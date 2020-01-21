//*************************************************************************************************
// Tree.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.GitApi.Internal;
using Newtonsoft.Json;
using Extensions = Microsoft.TeamFoundation.GitApi.Internal.Extensions;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Object-model representation of a Git tree object.
    /// </summary>
    public interface ITree : IObject, IEquatable<ITree>
    {
        /// <summary>
        /// Gets a list of blobs, reachable from this tree, as named objects.
        /// </summary>
        IReadOnlyList<INamedObject<IBlob>> Blobs { get; }

        /// <summary>
        /// Gets this tree's parent tree, if known; otherwise `<see langword="null"/>`.
        /// </summary>
        ITree Parent { get; }

        /// <summary>
        /// Gets a list of trees, reachable from this tree, as named object.
        /// </summary>
        IReadOnlyList<INamedObject<ITree>> Trees { get; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    internal class Tree : ObjectBase, IEquatable<Tree>, ITree
    {
        public static readonly StringComparer NameComparer = StringComparer.Ordinal;

        private static readonly string ParseExceptionMessage = $"Parse error: invalid `{typeof(Tree).Name}` data.";

        public Tree(ObjectHeader header)
            : base(header, ObjectType.Tree)
        {
            _blobs = null;
            _parent = null;
            _trees = null;
        }

        public Tree(ITree parent, ObjectHeader header)
            : base(header, ObjectType.Tree)
        {
            _blobs = null;
            _parent = parent;
            _trees = null;
        }

        private List<INamedObject<IBlob>> _blobs;
        private ITree _parent;
        private List<INamedObject<ITree>> _trees;

        [JsonProperty]
        public IReadOnlyList<INamedObject<IBlob>> Blobs
        {
            get { return _blobs; }
        }

        [JsonProperty]
        public ITree Parent
        {
            get { return _parent; }
        }

        // Don't serialize this as it makes circular references. If it's important, we'll need a new method on IRepository to access children.
        //[JsonProperty]
        public IReadOnlyList<INamedObject<ITree>> Trees
        {
            get { return _trees; }
        }

        public bool Equals(Tree other)
            => Comparer.Equals(this, other);

        public bool Equals(ITree other)
            => Comparer.Equals(this, other);

        public override bool Equals(object obj)
        {
            return Equals(obj as Tree)
                || Equals(obj as ITree)
                || base.Equals(obj);
        }

        public override int GetHashCode()
            => base.GetHashCode();

        public override string ToString()
            => base.ToString();

        internal void SetData(List<INamedObject<IBlob>> blobs, ITree parent, List<INamedObject<ITree>> trees)
        {
            _blobs = blobs;
            _parent = parent;
            _trees = trees;
        }

        internal unsafe override void ParseData(ByteBuffer buffer, ref int index, int count, int skipPrefix, INamedObjectFilter filter)
        {
            if (_blobs == null)
            {
                _blobs = new List<INamedObject<IBlob>>();
            }
            if (_trees == null)
            {
                _trees = new List<INamedObject<ITree>>();
            }

            INamedObjectFilterUtf8 filterUtf8 = filter as INamedObjectFilterUtf8;

            int get = index;
            int end = index + count;

            while (get < end)
            {
                while (get < end && buffer[get] == '\0')
                {
                    get += 1;
                }

                int idx = buffer.FirstIndexOf(' ', get, end - get);
                if (idx < 0)
                    throw new ObjectParseException("treeSpace", new StringUtf8(buffer, index, count), get - index);

                uint mode;
                if (!Extensions.TryParse(buffer, get, idx - get, out mode))
                    throw new ObjectParseException("treeMode", new StringUtf8(buffer, index, count), get - index);

                get = idx + 1;

                ObjectType type;

                switch (mode)
                {
                    case 40000:
                        type = ObjectType.Tree;
                        break;

                    case 100644:
                    case 100664:
                    case 100755:
                    case 120000:
                        type = ObjectType.Blob;
                        break;

                    case 160000:
                        type = ObjectType.Submodule;
                        break;

                    default:
                        throw new ObjectParseException("treeUnknownMode", new StringUtf8(buffer, index, count), get - index);
                }

                idx = buffer.FirstIndexOf('\0', get, end - get);
                if (idx < 0)
                    throw new ObjectParseException("treeEol", new StringUtf8(buffer, index, count), get - index);

                StringUtf8 name = new StringUtf8(buffer, get, idx - get);

                get = idx + 1;

                // if a child comparer was provided, check to see if this child should be
                // included in the tree/blob child collections
                if ((filterUtf8 != null && filterUtf8.Equals(name))
                    || (filter == null || filter.Equals((string)name)))
                {
                    ObjectId objectId = ObjectId.FromBytes(buffer, get);

                    switch (type)
                    {
                        case ObjectType.Blob:
                            {
                                Blob blob = new Blob(new ObjectHeader(objectId, type));
                                blob.SetContextAndCache(Context, _cache);
                                INamedObject<IBlob> namedBlob = new NamedObject<IBlob>(name, blob);

                                _blobs.Add(namedBlob);
                            }
                            break;

                        case ObjectType.Tree:
                            {
                                Tree tree = new Tree(this, new ObjectHeader(objectId, type));
                                tree.SetContextAndCache(Context, _cache);
                                INamedObject<ITree> namedTree = new NamedObject<ITree>(name, tree);

                                _trees.Add(namedTree);
                            }
                            break;

                        case ObjectType.Submodule:
                            // ISubmodule module = new Submodule(this, objectId.Value, name);
                            // moduleList.Add(module);
                            break;

                        default:
                            throw new ObjectParseException("treeUnknownObjectType", new StringUtf8(buffer, index, count), get - index);
                    }
                }

                get = idx + sizeof(ObjectId) + 1;
            }

            index = get;
        }
    }
}
