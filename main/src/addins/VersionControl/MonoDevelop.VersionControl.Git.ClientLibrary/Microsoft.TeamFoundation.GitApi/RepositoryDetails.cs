//*************************************************************************************************
// RepositoryDetails.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using Newtonsoft.Json;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Object-model structure containing details related to an instance of `<see cref="IRepository"/>`.
    /// </summary>
    public interface IRepositoryDetails
    {
        /// <summary>
        /// The path to the repository's common directory (usually `<see cref="GitDirectory"/>`).
        /// <para/>
        /// This value represents the path, but not guarantee the existence of the path.
        /// </summary>
        string CommonDirectory { get; }

        /// <summary>
        /// The path to the repository's description file (usually .git/description).
        /// <para/>
        /// This value represents the path, but not guarantee the existence of the path.
        /// </summary>
        string DescriptionFile { get; }

        /// <summary>
        /// The root of the repository's .git/ directory (aka $GIT_DIR).
        /// </summary>
        string GitDirectory { get; }

        /// <summary>
        /// The root of the repository's hook directory (usually .git/hooks).
        /// <para/>
        /// This value represents the path, but not guarantee the existence of the path.
        /// </summary>
        string HooksDirectory { get; }

        /// <summary>
        /// The root of the repository's info directory (usually .git/info).
        /// <para/>
        /// This value represents the path, but not guarantee the existence of the path.
        /// </summary>
        string InfoDirectory { get; }

        /// <summary>
        /// The path to the repository's index file (usually .git/index).
        /// <para/>
        /// This value represents the path, but not guarantee the existence of the path.
        /// </summary>
        string IndexFile { get; }

        /// <summary>
        /// The root of the repository's log directory (usually .git/logs).
        /// <para/>
        /// This value represents the path, but not guarantee the existence of the path.
        /// </summary>
        string LogsDirectory { get; }

        /// <summary>
        /// The root of the repository's object directory (usually .git/objects).
        /// <para/>
        /// This value represents the path, but not guarantee the existence of the path.
        /// </summary>
        string ObjectsDirectory { get; }

        /// <summary>
        /// The path to the repository's shared index file, if any (usually `<see langword="null"/>`).
        /// <para/>
        /// This value represents the path, but not guarantee the existence of the path.
        /// </summary>
        string SharedIndexFile { get; }

        /// <summary>
        /// The root of the repository's working directory (aka $WORK_DIR).
        /// </summary>
        string WorkingDirectory { get; }

        /// <summary>
        /// `<see langword="true"/>` if the repository is bare; otherwise `<see langword="false"/>`.
        /// </summary>
        bool IsBareRepository { get; }
    }

    /// <summary>
    /// Contains details related to an instance of <see cref="Repository"/>.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class RepositoryDetails : IRepositoryDetails
    {
        internal RepositoryDetails(string commonDirectory,
                                   string descriptionFile,
                                   string gitDirectory,
                                   string hooksDirectory,
                                   string infoDirectory,
                                   string indexFile,
                                   string logsDirectory,
                                   string objectsDirectory,
                                   string sharedIndexFile,
                                   string workingDirectory,
                                   bool isBareRepository)
        {
            CommonDirectory = commonDirectory;
            DescriptionFile = descriptionFile;
            GitDirectory = gitDirectory;
            HooksDirectory = hooksDirectory;
            InfoDirectory = infoDirectory;
            IndexFile = indexFile;
            LogsDirectory = logsDirectory;
            ObjectsDirectory = objectsDirectory;
            SharedIndexFile = sharedIndexFile;
            WorkingDirectory = workingDirectory;
            IsBareRepository = isBareRepository;
        }

        /// <summary>
        /// <para>The path to the repository's common directory (usually <see cref="GitDirectory"/>).</para>
        /// <para>This value represents the path, but not guarantee the existence of the path.</para>
        /// </summary>
        [JsonProperty]
        public string CommonDirectory { get; private set; }
        /// <summary>
        /// <para>The path to the repository's description file (usually .git/description)</para>
        /// <para>This value represents the path, but not guarantee the existence of the path.</para>
        /// </summary>
        [JsonProperty]
        public string DescriptionFile { get; private set; }
        /// <summary>
        /// The root of the repository's .git/ directory (aka $GIT_DIR)
        /// </summary>
        [JsonProperty]
        public string GitDirectory { get; private set; }
        /// <summary>
        /// <para>The root of the repository's hook directory (usually .git/hooks)</para>
        /// <para>This value represents the path, but not guarantee the existence of the path.</para>
        /// </summary>
        [JsonProperty]
        public string HooksDirectory { get; private set; }
        /// <summary>
        /// <para>The root of the repository's info directory (usually .git/info)</para>
        /// <para>This value represents the path, but not guarantee the existence of the path.</para>
        /// </summary>
        [JsonProperty]
        public string InfoDirectory { get; private set; }
        /// <summary>
        /// <para>The path to the repository's index file (usually .git/index)</para>
        /// <para>This value represents the path, but not guarantee the existence of the path.</para>
        /// </summary>
        [JsonProperty]
        public string IndexFile { get; private set; }
        /// <summary>
        /// <para>The root of the repository's log directory (usually .git/logs)</para>
        /// <para>This value represents the path, but not guarantee the existence of the path.</para>
        /// </summary>
        [JsonProperty]
        public string LogsDirectory { get; private set; }
        /// <summary>
        /// <para>The root of the repository's object directory (usuaully .git/objects)</para>
        /// <para>This value represents the path, but not guarantee the existence of the path.</para>
        /// </summary>
        [JsonProperty]
        public string ObjectsDirectory { get; private set; }
        /// <summary>
        /// <para>The path to the repository's shared index file, if any (usually <see langword="null"/>).</para>
        /// <para>This value represents the path, but not guarantee the existence of the path.</para>
        /// </summary>
        [JsonProperty]
        public string SharedIndexFile { get; private set; }
        /// <summary>
        /// The root of the repository's working directory (aka $WORK_DIR)
        /// </summary>
        [JsonProperty]
        public string WorkingDirectory { get; private set; }
        /// <summary>
        /// <see langword="true"/> if the repository is bare; otherwise <see langword="false"/>.
        /// </summary>
        [JsonProperty]
        public bool IsBareRepository { get; private set; }
    }
}
