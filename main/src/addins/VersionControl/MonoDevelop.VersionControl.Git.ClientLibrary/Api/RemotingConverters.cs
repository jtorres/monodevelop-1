//*************************************************************************************************
// RemotingConverters.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.TeamFoundation.GitApi.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.TeamFoundation.GitApi
{
    public class RemotingBaseConverter : JsonConverter
    {
        public RemotingBaseConverter()
            : base()
        {
        }

        public override bool CanConvert(Type objectType)
        {
            if (objectType.Equals(typeof(string)))
            {
                return true;
            }
            else if (objectType.Equals(typeof(ObjectId)))
            {
                return true;
            }

            return false;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (objectType.Equals(typeof(string)))
            {
                // Don't modify strings on reads
                Debug.Assert(reader.TokenType == JsonToken.String || reader.TokenType == JsonToken.Null);
                return (string)reader.Value;
            }
            else if (objectType.Equals(typeof(ObjectId)))
            {
                Debug.Assert(reader.TokenType == JsonToken.String);
                return ObjectId.FromString((string)reader.Value);
            }

            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is string)
            {
                string str = (string)value;

                writer.WriteValue(str);
            }
            else if (value is ObjectId)
            {
                serializer.Serialize(writer, ((ObjectId)value).RevisionText);
            }
        }
    }

    public class RemotingMainConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            // TODO: Hashtable if we keep this
            if (objectType.Equals(typeof(TreeDifferenceDetail)) ||
                objectType.Equals(typeof(ConfigurationEntry)) ||
                objectType.Equals(typeof(ObjectHeader)) ||
                objectType.Equals(typeof(IRemote)) ||
                objectType.Equals(typeof(Remote)) ||
                objectType.Equals(typeof(IRepositoryDetails)) ||
                objectType.Equals(typeof(RepositoryDetails)) ||
                objectType.Equals(typeof(IStatusBranchInfo)) ||
                objectType.Equals(typeof(StatusBranchInfo)) ||
                objectType.Equals(typeof(IStatusSnapshot)) ||
                objectType.Equals(typeof(StatusSnapshot)) ||
                objectType.Equals(typeof(IReference)) ||
                objectType.Equals(typeof(IRevision)) ||
                objectType.Equals(typeof(IBranch)) ||
                objectType.Equals(typeof(Branch)) ||
                objectType.Equals(typeof(IHead)) ||
                objectType.Equals(typeof(Head)) ||
                objectType.Equals(typeof(ICommit)) ||
                objectType.Equals(typeof(Commit)) ||
                objectType.Equals(typeof(ITag)) ||
                objectType.Equals(typeof(Tag)) ||
                objectType.Equals(typeof(ITagAnnotation)) ||
                objectType.Equals(typeof(TagAnnotation)) ||
                objectType.Equals(typeof(ITree)) ||
                objectType.Equals(typeof(Tree)) ||
                objectType.Equals(typeof(ITreeDifference)) ||
                objectType.Equals(typeof(IUpdatedIndexEntry)) ||
                objectType.Equals(typeof(UpdatedIndexEntry)) ||
                objectType.Equals(typeof(IUpdatedWorktreeEntry)) ||
                objectType.Equals(typeof(UpdatedWorktreeEntry)) ||
                objectType.Equals(typeof(Revision)) ||
                objectType.Equals(typeof(StatusTrackedEntries)) ||
                objectType.Equals(typeof(StatusUnmergedEntry)) ||
                objectType.Equals(typeof(IStatusTrackedEntries)) ||
                objectType.Equals(typeof(IStatusUnmergedEntry)))
            {
                return true;
            }

            return false;
        }

        public override bool CanWrite
        {
            get
            {
                // Do not write anything since structs can already be serialized.
                return false;
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // TODO: Use IsAssignableFrom to tighten the cases up a bit

            // Since these types are structs, the default parameterless ctor
            // is being used, resulting in empty deserialized objects
            if (objectType.Equals(typeof(TreeDifferenceDetail)))
            {
                JObject obj = JObject.Load(reader);
                return ReadTreeDifferenceDetail(obj);

            }
            else if (objectType.Equals(typeof(ConfigurationEntry)))
            {
                JObject obj = JObject.Load(reader);
                return new ConfigurationEntry(obj[nameof(ConfigurationEntry.Key)].Value<string>(),
                                           obj[nameof(ConfigurationEntry.Value)].Value<string>(),
                                           (ConfigurationLevel)obj[nameof(ConfigurationEntry.Level)].Value<int>(),
                                           obj[nameof(ConfigurationEntry.Source)].Value<string>());
            }
            else if (objectType.Equals(typeof(ObjectHeader)))
            {
                JObject obj = JObject.Load(reader);
                return new ObjectHeader(ObjectId.FromString(obj[nameof(ObjectHeader.ObjectId)].Value<string>()),
                                           (ObjectType)obj[nameof(ObjectHeader.Type)].Value<int>(),
                                           (long)obj[nameof(ObjectHeader.Size)].Value<int>());
            }
            else if (objectType.Equals(typeof(IStatusTrackedEntries)))
            {
                IEnumerable<JToken> entries = null;

                if (reader.TokenType == JsonToken.StartObject)
                {
                    JObject obj = JObject.Load(reader);
                    Debug.Assert(obj["$values"]?.Type == JTokenType.Array);

                    entries = obj["$values"];
                }
                else if (reader.TokenType == JsonToken.StartArray)
                {
                    entries = JArray.Load(reader);
                }
                else
                {
                    throw new InvalidDataException();
                }

                var statusTrackedEntries = new StatusTrackedEntries();

                foreach (var entry in entries)
                {
                    StatusEntry statusEntry = ReadStatusEntry(entry);

                    statusTrackedEntries.Add(statusEntry);
                }

                return statusTrackedEntries;
            }
            else if (objectType.Equals(typeof(IStatusUnmergedEntry)))
            {
                JObject obj = JObject.Load(reader);
                return ReadStatusUnmergedEntry(obj);
            }
            else if (objectType.Equals(typeof(IRemote)))
            {
                JObject obj = JObject.Load(reader);
                return new Remote((StringUtf8)(obj[nameof(IRemote.Name)].Value<string>()),
                    (StringUtf8)(obj[nameof(IRemote.FetchUrl)].Value<string>()),
                    (StringUtf8)(obj[nameof(IRemote.PushUrl)].Value<string>()));
            }
            else if (objectType.Equals(typeof(IRepositoryDetails)))
            {
                JObject obj = JObject.Load(reader);
                return new RepositoryDetails(obj[nameof(IRepositoryDetails.CommonDirectory)].Value<string>(),
                    obj[nameof(IRepositoryDetails.DescriptionFile)].Value<string>(),
                    obj[nameof(IRepositoryDetails.GitDirectory)].Value<string>(),
                    obj[nameof(IRepositoryDetails.HooksDirectory)].Value<string>(),
                    obj[nameof(IRepositoryDetails.InfoDirectory)].Value<string>(),
                    obj[nameof(IRepositoryDetails.IndexFile)].Value<string>(),
                    obj[nameof(IRepositoryDetails.LogsDirectory)].Value<string>(),
                    obj[nameof(IRepositoryDetails.ObjectsDirectory)].Value<string>(),
                    obj[nameof(IRepositoryDetails.SharedIndexFile)].Value<string>(),
                    obj[nameof(IRepositoryDetails.WorkingDirectory)].Value<string>(),
                    obj[nameof(IRepositoryDetails.IsBareRepository)].Value<bool>());
            }
            else if (objectType.Equals(typeof(IStatusSnapshot)))
            {
                JObject obj = JObject.Load(reader);
                var branchInfo = ReadStatusBranchInfo(obj[nameof(IStatusSnapshot.BranchInfo)] as JObject);
                var ignoredItems = obj[nameof(IStatusSnapshot.IgnoredItems)].ToObject<List<string>>();

                var trackedItemsJson = obj[nameof(IStatusSnapshot.TrackedItems)]["$values"];
                StatusTrackedEntries trackedEntries = new StatusTrackedEntries();
                foreach (var item in trackedItemsJson)
                {
                    trackedEntries.Add(ReadStatusEntry(item));
                }

                var unmergedItemsJson = obj[nameof(IStatusSnapshot.UnmergedItems)]["$values"];
                List<IStatusUnmergedEntry> unmergedItems = new List<IStatusUnmergedEntry>();
                foreach (var item in unmergedItemsJson)
                {
                    unmergedItems.Add(ReadStatusUnmergedEntry(item));
                }

                var untrackedItems = obj[nameof(IStatusSnapshot.UntrackedItems)].ToObject<List<string>>();

                return new StatusSnapshot(branchInfo, ignoredItems, trackedEntries, unmergedItems, untrackedItems);
            }
            else if (objectType.Equals(typeof(IStatusBranchInfo)) || objectType.Equals(typeof(StatusBranchInfo)))
            {
                JObject obj = JObject.Load(reader);
                return ReadStatusBranchInfo(obj);
            }
            else if (objectType.Equals(typeof(IReference)))
            {
                JObject obj = JObject.Load(reader);
                return ReadIReference(obj);
            }
            else if (objectType.Equals(typeof(IRevision)))
            {
                if (reader.TokenType == JsonToken.String)
                {
                    return ObjectId.FromString((string)reader.Value);
                }
                else if (reader.TokenType == JsonToken.Null)
                {
                    return null;
                }

                JObject obj = JObject.Load(reader);
                if (obj["$type"]?.Value<string>()?.Contains(nameof(Branch)) ?? false)
                {
                    return ReadBranch(obj);
                }
                else if (obj["$type"]?.Value<string>()?.Contains(nameof(Head)) ?? false)
                {
                    return ReadHead(obj);
                }
                else if (obj["$type"]?.Value<string>()?.Contains(nameof(Commit)) ?? false)
                {
                    return ReadCommit(obj);
                }
                else if (obj["$type"]?.Value<string>()?.Contains(nameof(TagAnnotation)) ?? false)
                {
                    return ReadTagAnnotation(obj);
                }
                else if (obj["$type"]?.Value<string>()?.Contains(nameof(Tree)) ?? false)
                {
                    return ReadTree(obj);
                }
                else if ((obj["$type"]?.Value<string>()?.Contains(nameof(Revision)) ?? false) || obj["RevisionText"] != null)
                {
                    return ReadRevision(obj);
                }
            }
            else if (objectType.Equals(typeof(IBranch)))
            {
                JObject obj = JObject.Load(reader);
                return ReadBranch(obj);
            }
            else if (objectType.Equals(typeof(IHead)))
            {
                JObject obj = JObject.Load(reader);
                return ReadHead(obj);
            }
            else if (objectType.Equals(typeof(ICommit)) || objectType.Equals(typeof(Commit)))
            {
                JObject obj = JObject.Load(reader);
                return ReadCommit(obj);
            }
            else if (objectType.Equals(typeof(ITree)) || objectType.Equals(typeof(Tree)))
            {
                JObject obj = JObject.Load(reader);
                return ReadTree(obj);
            }
            else if (objectType.Equals(typeof(ITreeDifference)))
            {
                JObject obj = JObject.Load(reader);
                return ReadTreeDifference(obj);
            }
            else if (objectType.Equals(typeof(IUpdatedIndexEntry)))
            {
                JObject obj = JObject.Load(reader);
                return ReadUpdatedIndexEntry(obj);
            }
            else if (objectType.Equals(typeof(IUpdatedWorktreeEntry)))
            {
                JObject obj = JObject.Load(reader);
                return ReadUpdatedWorktreeEntry(obj);
            }

            return null;
        }

        private Branch ReadBranch(JObject obj)
        {
            if (obj == null)
            {
                return null;
            }

            var branch = new Branch((StringUtf8)(obj[nameof(IBranch.CanonicalName)].Value<string>()),
                ObjectId.FromString(obj[nameof(IBranch.ObjectId)].Value<string>()),
                (ObjectType)Enum.Parse(typeof(ObjectType), obj[nameof(IBranch.ObjectType)].Value<string>()));

            branch.IsHead = obj[nameof(IBranch.IsHead)].Value<bool>();
            branch.PushTargetNameUtf8 = (StringUtf8)(obj[nameof(IBranch.PushTargetName)].Value<string>());
            branch.UpstreamNameUtf8 = (StringUtf8)(obj[nameof(IBranch.UpstreamName)].Value<string>());
            return branch;
        }

        private Commit ReadCommit(JObject obj)
        {
            if (obj == null)
            {
                return null;
            }

            var header = new ObjectHeader(ObjectId.FromString(obj[nameof(ICommit.ObjectId)].Value<string>()),
                (ObjectType)obj[nameof(ICommit.ObjectType)].Value<int>(),
                obj[nameof(ICommit.Size)].Value<int>());
            var commit = new Commit(header);

            List<ObjectId> parentIdentities = new List<ObjectId>();
            if (obj.TryGetValue(nameof(ICommit.ParentIdentities), out JToken parents))
            {
                foreach (string item in parents["$values"])
                {
                    parentIdentities.Add(ObjectId.FromString(item));
                }
            }

            commit.SetData(ReadIdentity(obj[nameof(ICommit.Author)] as JObject),
                ReadIdentity(obj[nameof(ICommit.Committer)] as JObject),
                (StringUtf8)(obj[nameof(ICommit.FirstLine)].Value<string>()),
                (StringUtf8)(obj[nameof(ICommit.Message)].Value<string>()),
                parentIdentities,
                ObjectId.FromString(obj[nameof(ICommit.TreeId)].Value<string>()));
            return commit;
        }

        private Head ReadHead(JObject obj)
        {
            if (obj == null)
            {
                return null;
            }

            Head head;

            if (obj.TryGetValue(nameof(Head.AsReference), out JToken reference))
            {
                head = new Head(new ReferenceCollection(ReferenceOptionFlags.Default), ReadIReference(reference as JObject));
            }
            else
            {
                head = new Head();
            }

            head.HeadType = (HeadType)obj[nameof(IHead.HeadType)].Value<int>();
            head.ObjectId = ObjectId.FromString(obj[nameof(IBranch.ObjectId)].Value<string>());
            head.ObjectType = (ObjectType)Enum.Parse(typeof(ObjectType), obj[nameof(IBranch.ObjectType)].Value<string>());
            head.CanonicalName = obj[nameof(IHead.CanonicalName)].Value<string>();
            head.FriendlyName = obj[nameof(IHead.FriendlyName)].Value<string>();

            if (obj.TryGetValue(nameof(Head.Commit), out JToken commit))
            {
                head.Commit = ReadCommit(commit as JObject);
            }

            return head;
        }

        private Identity ReadIdentity(JObject obj)
        {
            if (obj == null)
            {
                return null;
            }

            return Identity.Create((StringUtf8)(obj[nameof(IIdentity.Username)].Value<string>()),
                (StringUtf8)(obj[nameof(IIdentity.Email)].Value<string>()),
                new DateTimeOffset(obj[nameof(IIdentity.Timestamp)].Value<DateTime>()));
        }

        private IReference ReadIReference(JObject obj)
        {
            if (obj == null)
            {
                return null;
            }

            if (obj["$type"]?.Value<string>()?.Contains(nameof(Branch)) ?? false)
            {
                return ReadBranch(obj);
            }
            else if (obj["$type"]?.Value<string>()?.Contains(nameof(Head)) ?? false)
            {
                return ReadHead(obj);
            }
            else if (obj["$type"]?.Value<string>()?.Contains(nameof(Tag)) ?? false)
            {
                return ReadTag(obj);
            }

            return null;
        }

        private Revision ReadRevision(JObject obj)
        {
            if (obj == null)
            {
                return null;
            }

            return new Revision(obj[nameof(IRevision.RevisionText)].Value<string>());
        }

        private StatusBranchInfo ReadStatusBranchInfo(JObject obj)
        {
            if (obj == null)
            {
                return null;
            }

            StatusBranchInfo info = new StatusBranchInfo();

            HeadType headType = (HeadType)obj[nameof(IStatusBranchInfo.HeadType)].Value<int>();

            if (headType == HeadType.Detached)
            {
                info.SetHeadIsDetached();
            }
            else if (headType == HeadType.Unknown)
            {
                info.SetHeadIsUnknown();
            }

            if (obj.TryGetValue(nameof(IStatusBranchInfo.HeadBranchName), out JToken headBranchName))
            {
                info.SetHeadBranchName((StringUtf8)(headBranchName.Value<string>()));
            }

            if (obj.TryGetValue(nameof(IStatusBranchInfo.CommitId), out JToken commitId))
            {
                info.CommitId = ObjectId.FromString(commitId.Value<string>());
            }

            if (obj.TryGetValue(nameof(IStatusBranchInfo.UpstreamBranchName), out JToken upstreamBranchName))
            {
                info.SetUpstreamBranchName((StringUtf8)(upstreamBranchName.Value<string>()));
            }

            if (obj.TryGetValue(nameof(IStatusBranchInfo.AheadBehind), out JToken aheadBehind))
            {
                var ab = (JObject)aheadBehind;
                if (ab.TryGetValue(nameof(AheadBehind.Ahead), out JToken ahead) && ab.TryGetValue(nameof(AheadBehind.Behind), out JToken behind))
                {
                    info.SetAheadBehindCounts(ahead.Value<int>(), behind.Value<int>());
                }
            }

            return info;
        }

        private StatusEntry ReadStatusEntry(JToken entry)
        {
            if (entry == null)
            {
                return null;
            }

            if ((entry["$type"]?.Value<string>()?.Contains(nameof(StatusEntry)) ?? false))
            {
                return new StatusEntry((TreeDifferenceType)entry[nameof(StatusEntry.IndexStatus)].Value<int>(),
                                                  (TreeDifferenceType)entry[nameof(StatusEntry.WorktreeStatus)].Value<int>(),
                                                  (StatusSubmoduleState)entry[nameof(StatusEntry.SubmoduleStatus)].Value<int>(),
                                                  (TreeEntryDetailMode)entry[nameof(StatusEntry.HeadMode)].Value<int>(),
                                                  (TreeEntryDetailMode)entry[nameof(StatusEntry.IndexMode)].Value<int>(),
                                                  (TreeEntryDetailMode)entry[nameof(StatusEntry.WorkTreeMode)].Value<int>(),
                                                  ObjectId.FromString(entry[nameof(StatusEntry.HeadSha)].Value<string>()),
                                                  ObjectId.FromString(entry[nameof(StatusEntry.IndexSha)].Value<string>()),
                                                  new StringUtf8(entry[nameof(StatusEntry.Path)].Value<string>()));
            }
            else if ((entry["$type"]?.Value<string>()?.Contains(nameof(StatusRenamedEntry)) ?? false))
            {
                return new StatusRenamedEntry((TreeDifferenceType)entry[nameof(StatusRenamedEntry.IndexStatus)].Value<int>(),
                                                         (TreeDifferenceType)entry[nameof(StatusRenamedEntry.WorktreeStatus)].Value<int>(),
                                                         (StatusSubmoduleState)entry[nameof(StatusRenamedEntry.SubmoduleStatus)].Value<int>(),
                                                         (TreeEntryDetailMode)entry[nameof(StatusRenamedEntry.HeadMode)].Value<int>(),
                                                         (TreeEntryDetailMode)entry[nameof(StatusRenamedEntry.IndexMode)].Value<int>(),
                                                         (TreeEntryDetailMode)entry[nameof(StatusRenamedEntry.WorkTreeMode)].Value<int>(),
                                                         ObjectId.FromString(entry[nameof(StatusRenamedEntry.HeadSha)].Value<string>()),
                                                         ObjectId.FromString(entry[nameof(StatusRenamedEntry.IndexSha)].Value<string>()),
                                                         entry[nameof(StatusRenamedEntry.Confidence)].Value<int>(),
                                                         new StringUtf8(entry[nameof(StatusRenamedEntry.OriginalPath)].Value<string>()),
                                                         new StringUtf8(entry[nameof(StatusRenamedEntry.CurrentPath)].Value<string>()));
            }
            else if ((entry["$type"]?.Value<string>()?.Contains(nameof(StatusCopiedEntry)) ?? false))
            {
                return new StatusCopiedEntry((TreeDifferenceType)entry[nameof(StatusCopiedEntry.IndexStatus)].Value<int>(),
                                                        (TreeDifferenceType)entry[nameof(StatusCopiedEntry.WorktreeStatus)].Value<int>(),
                                                        (StatusSubmoduleState)entry[nameof(StatusCopiedEntry.SubmoduleStatus)].Value<int>(),
                                                        (TreeEntryDetailMode)entry[nameof(StatusCopiedEntry.HeadMode)].Value<int>(),
                                                        (TreeEntryDetailMode)entry[nameof(StatusCopiedEntry.IndexMode)].Value<int>(),
                                                        (TreeEntryDetailMode)entry[nameof(StatusCopiedEntry.WorkTreeMode)].Value<int>(),
                                                        ObjectId.FromString(entry[nameof(StatusCopiedEntry.HeadSha)].Value<string>()),
                                                        ObjectId.FromString(entry[nameof(StatusCopiedEntry.IndexSha)].Value<string>()),
                                                        new StringUtf8(entry[nameof(StatusRenamedEntry.OriginalPath)].Value<string>()),
                                                        new StringUtf8(entry[nameof(StatusRenamedEntry.CurrentPath)].Value<string>()),
                                                        entry[nameof(StatusCopiedEntry.Confidence)].Value<int>());
            }
            else
            {
                throw new InvalidOperationException("Unknown type: \"" + (entry["$type"]?.Value<string>() ?? "null") + "\".");
            }
        }

        private StatusUnmergedEntry ReadStatusUnmergedEntry(JToken obj)
        {
            return new StatusUnmergedEntry((StatusUnmergedState)obj[nameof(StatusUnmergedEntry.UnmergedState)].Value<int>(),
                                               (StatusSubmoduleState)obj[nameof(StatusUnmergedEntry.SubmoduleStatus)].Value<int>(),
                                               (TreeEntryDetailMode)obj[nameof(StatusUnmergedEntry.AncestorMode)].Value<int>(),
                                               (TreeEntryDetailMode)obj[nameof(StatusUnmergedEntry.OursMode)].Value<int>(),
                                               (TreeEntryDetailMode)obj[nameof(StatusUnmergedEntry.TheirsMode)].Value<int>(),
                                               (TreeEntryDetailMode)obj[nameof(StatusUnmergedEntry.WorktreeMode)].Value<int>(),
                                               ObjectId.FromString(obj[nameof(StatusUnmergedEntry.AncestorSha)].Value<string>()),
                                               ObjectId.FromString(obj[nameof(StatusUnmergedEntry.OursSha)].Value<string>()),
                                               ObjectId.FromString(obj[nameof(StatusUnmergedEntry.ThiersSha)].Value<string>()),
                                               new StringUtf8(obj[nameof(StatusUnmergedEntry.Path)].Value<string>()));
        }

        private Tag ReadTag(JObject obj)
        {
            if (obj == null)
            {
                return null;
            }

            var tag = new Tag((StringUtf8)(obj[nameof(ITag.CanonicalName)].Value<string>()),
                ObjectId.FromString(obj[nameof(ITag.ObjectId)].Value<string>()),
                (ObjectType)Enum.Parse(typeof(ObjectType), obj[nameof(ITag.ObjectType)].Value<string>()));

            if (obj.TryGetValue(nameof(ITag.Annotation), out JToken annotation))
            {
                tag.Annotation = ReadTagAnnotation(annotation as JObject);
            }

            return tag;
        }

        private TagAnnotation ReadTagAnnotation(JObject obj)
        {
            if (obj == null)
            {
                return null;
            }

            var header = new ObjectHeader(ObjectId.FromString(obj[nameof(ITagAnnotation.ObjectId)].Value<string>()),
                (ObjectType)obj[nameof(ITagAnnotation.ObjectType)].Value<int>(),
                obj[nameof(ITagAnnotation.Size)].Value<int>());
            var tagAnnotation = new TagAnnotation(header);

            tagAnnotation.SetData((StringUtf8)(obj[nameof(ITagAnnotation.FriendlyName)].Value<string>()),
                ObjectId.FromString(obj[nameof(ITagAnnotation.ObjectId)].Value<string>()),
                (ObjectType)obj[nameof(ITagAnnotation.TargetType)].Value<int>(),
                ReadIdentity(obj[nameof(ITagAnnotation.Tagger)] as JObject),
                (StringUtf8)(obj[nameof(ITagAnnotation.FirstLine)].Value<string>()),
                (StringUtf8)(obj[nameof(ITagAnnotation.Message)].Value<string>())
                );

            return tagAnnotation;
        }

        private Tree ReadTree(JObject obj)
        {
            if (obj == null)
            {
                return null;
            }

            var header = new ObjectHeader(ObjectId.FromString(obj[nameof(ITree.ObjectId)].Value<string>()),
                (ObjectType)obj[nameof(ITree.ObjectType)].Value<int>(),
                obj[nameof(ITree.Size)].Value<int>());
            var tree = new Tree(header);

            List<INamedObject<IBlob>> blobs = new List<INamedObject<IBlob>>(/*obj[nameof(ITree.Blobs)].ToObject<INamedObject<IBlob>[]>()*/);
            ITree parent = obj.TryGetValue(nameof(ITree.Parent), out JToken treeToken) ? treeToken.ToObject<ITree>() : null;
            List<INamedObject<ITree>> trees = new List<INamedObject<ITree>>(/*obj[nameof(ITree.Trees)].ToObject<INamedObject<ITree>[]>()*/);

            tree.SetData(blobs, parent, trees);

            return tree;
        }

        private TreeDifference ReadTreeDifference(JObject obj)
        {
            if (obj == null)
            {
                return null;
            }

            var treeDifference = new TreeDifference();

            foreach (var child in obj[nameof(ITreeDifference.Entries)]["$values"])
            {
                treeDifference.AddEntry(ReadTreeDifferenceEntry(child as JObject));
            }

            return treeDifference;
        }

        private TreeDifferenceDetail ReadTreeDifferenceDetail(JObject obj)
        {
            if (obj == null)
            {
                return null;
            }

            return new TreeDifferenceDetail(ObjectId.FromString(obj[nameof(TreeDifferenceDetail.ObjectId)].Value<string>()),
                                                (TreeEntryDetailMode)obj[nameof(TreeDifferenceDetail.Mode)].Value<int>(),
                                                (TreeDifferenceType)obj[nameof(TreeDifferenceDetail.Type)].Value<int>());
        }

        private TreeDifferenceEntry ReadTreeDifferenceEntry(JObject obj)
        {
            if (obj == null)
            {
                return null;
            }

            string path = obj[nameof(ITreeDifferenceEntry.Path)].Value<string>();

            var sourcesJson = obj[nameof(ITreeDifferenceEntry.Sources)]["$values"];
            List<ITreeDifferenceDetail> sources = new List<ITreeDifferenceDetail>();
            foreach (var item in sourcesJson)
            {
                sources.Add(ReadTreeDifferenceDetail(item as JObject));
            }

            ITreeDifferenceDetail target = ReadTreeDifferenceDetail(obj[nameof(ITreeDifferenceEntry.Target)] as JObject);
            bool copied = (obj["$type"]?.Value<string>()?.Contains(nameof(TreeDifferenceCopiedEntry)) ?? false);
            bool renamed = (obj["$type"]?.Value<string>()?.Contains(nameof(TreeDifferenceRenamedEntry)) ?? false);

            if (copied || renamed)
            {
                // looks weird to always use Copied but ITreeDifferenceCopiedEntry and ITreeDifferenceRenamedEntry are exactly the same shape
                int confidence = obj[nameof(ITreeDifferenceCopiedEntry.Confidence)].Value<int>();
                string originalPath = obj[nameof(ITreeDifferenceCopiedEntry.OriginalPath)].Value<string>();

                if (copied)
                {
                    return new TreeDifferenceCopiedEntry((StringUtf8)(originalPath), new StringUtf8(path), confidence, target, sources.ToArray());
                }
                else
                {
                    return new TreeDifferenceRenamedEntry((StringUtf8)(originalPath), new StringUtf8(path), confidence, target, sources.ToArray());
                }
            }
            else
            {
                return new TreeDifferenceEntry((StringUtf8)path, target, sources.ToArray());
            }
        }

        private UpdatedIndexEntry ReadUpdatedIndexEntry(JObject obj)
        {
            if (obj == null)
            {
                return null;
            }

            string path = obj[nameof(IUpdatedIndexEntry.Path)].Value<string>();
            TreeDifferenceType type = (TreeDifferenceType)obj[nameof(IUpdatedIndexEntry.Type)].Value<int>();
            return new UpdatedIndexEntry(new StringUtf8(path), type);
        }

        private UpdatedWorktreeEntry ReadUpdatedWorktreeEntry(JObject obj)
        {
            if (obj == null)
            {
                return null;
            }

            string path = obj[nameof(IUpdatedWorktreeEntry.Path)].Value<string>();
            string details = obj[nameof(IUpdatedWorktreeEntry.Details)].Value<string>();
            UpdatedWorktreeEntryType type = (UpdatedWorktreeEntryType)obj[nameof(IUpdatedWorktreeEntry.Type)].Value<int>();
            return new UpdatedWorktreeEntry(new StringUtf8(path), type, new StringUtf8(details));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // This converter opts out of writing (CanWrite=false) since the structs can
            // generally be serialized fine
        }
    }
}
