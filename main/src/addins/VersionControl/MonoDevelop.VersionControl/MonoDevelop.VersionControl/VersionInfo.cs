
using MonoDevelop.Core;
using System.Threading;
using System.Threading.Tasks;

namespace MonoDevelop.VersionControl
{

	public class VersionInfo
	{
		bool opsLoaded;
		VersionControlOperation operations;
		Repository ownerRepository;

		public VersionInfo (FilePath localPath, string repositoryPath, bool isDirectory, VersionStatus status, Revision revision, Revision remoteRevision)
		{
			this.LocalPath = localPath;
			this.RepositoryPath = repositoryPath;
			this.IsDirectory = isDirectory;
			this.Status = status;
			this.Revision = revision;
			this.RemoteRevision = remoteRevision;
		}

		public bool Equals (VersionInfo obj)
		{
			if (obj == null)
				return false;
			return LocalPath == obj.LocalPath &&
				RepositoryPath == obj.RepositoryPath &&
				IsDirectory == obj.IsDirectory &&
				Status.Equals (obj.Status) &&
				Revision == obj.Revision &&
				RemoteRevision == obj.RemoteRevision &&
				AllowedOperations == obj.AllowedOperations;
		}
		
		internal async Task InitAsync (Repository repo, CancellationToken cancellationToken = default)
		{
			ownerRepository = repo;
			RequiresRefresh = false;
			operations = await ownerRepository.GetSupportedOperationsAsync (this, cancellationToken).ConfigureAwait (false);
		}
		
		public static VersionInfo CreateUnversioned (FilePath path, bool isDirectory)
		{
			return new VersionInfo (path, "", isDirectory, VersionStatus.Unversioned, null, null);
		}

		internal bool RequiresRefresh { get; set; }

		internal bool IsInitialized => ownerRepository != null;

		public bool IsVersioned {
			get { return Status.IsTracked; }
		}
		
		public bool HasLocalChanges {
			get { return Status.IsModified; }
		}
		
		public bool HasRemoteChanges {
			get { return Status.IsRemoteModified; }
		}
		
		public FilePath LocalPath {
			get;
			private set;
		}
		
		public string RepositoryPath {
			get;
			private set;
		}
		
 		public bool IsDirectory {
			get;
			private set;
		}
 		
		public Revision Revision {
			get;
			private set;
		}

		public VersionStatus Status {
			get;
			private set;
		}

		public Revision RemoteRevision {
			get;
			internal set;
		}
		
		public VersionControlOperation AllowedOperations {
			get {
				return operations;
			}
		}
		
		public bool SupportsOperation (VersionControlOperation op)
		{
			return (AllowedOperations & op) != 0;
		}
		
		public bool CanAdd { get { return SupportsOperation (VersionControlOperation.Add); } }
		
		public bool CanAnnotate { get { return SupportsOperation (VersionControlOperation.Annotate); } }
		
		public bool CanCommit { get { return SupportsOperation (VersionControlOperation.Commit); } }
		
		public bool CanLock { get { return SupportsOperation (VersionControlOperation.Lock); } }
		
		public bool CanLog { get { return SupportsOperation (VersionControlOperation.Log); } }
		
		public bool CanRemove { get { return SupportsOperation (VersionControlOperation.Remove); } }
		
		public bool CanRevert { get { return SupportsOperation (VersionControlOperation.Revert); } }
		
		public bool CanUnlock { get { return SupportsOperation (VersionControlOperation.Unlock); } }
		
		public bool CanUpdate { get { return SupportsOperation (VersionControlOperation.Update); } }

		public override string ToString ()
		{
			return $"[VersionInfo: LocalPath={LocalPath}]";
		}
	}
}
