using System.IO;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Object-model structure containing state related to an instance of `<see cref="IRepository"/>`.
    /// </summary>
    interface IRepositoryCurrentState
    {
        /// <summary>
        /// Read the current MERGE_MSG file, if it exists. This is used by various git commands
        /// during an operation.
        /// <para/>
        /// Returns the current merge message if successful; otherwise `<see langword="null"/>`.
        string ReadCurrentMergeMessage();

        /// <summary>
        /// Manually scan the .git directory to determine if we are in a current operation.
        /// <para/>
        /// Note that this looks for a series of special files, like ".git/MERGE_HEAD" to infer the
        /// current operation, so it should be considered fragile. The order of operations is very
        /// important and should match that in git. For example, rebase must be checked before
        /// cherry-pick because rebase may use cherry-pick while applying commits.
        /// <para/>
        /// The current order in git is merge, rebase, cherry-pick, bisect, revert.
        /// </summary>
        RepositoryCurrentOperation ReadCurrentOperation();

        /// <summary>
        /// Determine if there is an index.lock present.
        /// </summary>
        bool ReadIsIndexCurrentlyLocked();
    }

    public class RepositoryCurrentState : Base, IRepositoryCurrentState
    {
        public RepositoryCurrentState(IExecutionContext context, IRepository repository)
            : base()
        {
            SetContext(context);
            _repository = repository;
        }

        IRepository _repository;

        public string ReadCurrentMergeMessage()
        {
            try
            {
                string gitDirectoryPath = _repository.GitDirectory;
                string msgPath = Path.Combine(gitDirectoryPath, "MERGE_MSG");
                if (FileSystem.FileExists(msgPath))
                {
                    using (StreamReader sr = new StreamReader(msgPath))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            catch
            { /* squelch */ }

            return null;
        }

        public RepositoryCurrentOperation ReadCurrentOperation()
        {
            string gitDirectoryPath = _repository.GitDirectory;
            string testPath = null;

            // test for the existance of .git/MERGE_HEAD file which signals a merge is in-progress
            if (FileSystem.FileExists(testPath = Path.Combine(gitDirectoryPath, "MERGE_HEAD")))
            {
                return RepositoryCurrentOperation.Merge;
            }
            // test for the existance of .git/rebase-apply folder which signals a rebase is in-progress
            else if (FileSystem.DirectoryExists(testPath = Path.Combine(gitDirectoryPath, "rebase-apply")))
            {
                // test for the .git/rebase-apply/applying file which signals git-am is in-progress
                // otherwise it is a normal rebase
                return FileSystem.FileExists(Path.Combine(testPath, "applying"))
                    ? RepositoryCurrentOperation.ApplyMailbox
                    : RepositoryCurrentOperation.Rebase;
            }
            // test for the existance of .git/rebase-merge folder which signals a rebase-merge is in-progress
            else if (FileSystem.DirectoryExists(testPath = Path.Combine(gitDirectoryPath, "rebase-merge")))
            {
                // test for the .git/rebase-merge/interactive file which signals rebase--interacitve is in-progress
                // otherwise it is a basic rebase-merge
                return FileSystem.FileExists(testPath = Path.Combine(testPath, "interactive"))
                    ? RepositoryCurrentOperation.RebaseInteractive
                    : RepositoryCurrentOperation.RebaseMerge;
            }
            // test for the existance of .git/CHERRY_PICK_HEAD file which signals a cherry-pick is in-progress
            else if (FileSystem.FileExists(testPath = Path.Combine(gitDirectoryPath, "CHERRY_PICK_HEAD")))
            {
                // cherry-pick come in two flavors: single and sequence
                // test for the existance of a .git/sequencer folder to determine which kind is in-progress
                // Per git core unit test "t/t3510-cherry-pick-sequence.sh", this is always ".git/sequencer/"
                // (and not under ".git/CHERRY_PICK_HEAD").
                return FileSystem.DirectoryExists(testPath = Path.Combine(gitDirectoryPath, "sequencer"))
                    ? RepositoryCurrentOperation.CherryPickSequence
                    : RepositoryCurrentOperation.CherryPick;
            }
            // test for the existance of .git/BISECT_START file which signals a bisect is in-progress
            else if (FileSystem.FileExists(testPath = Path.Combine(gitDirectoryPath, "BISECT_START")))
            {
                return RepositoryCurrentOperation.Bisect;
            }
            else if (FileSystem.FileExists(testPath = Path.Combine(gitDirectoryPath, "REVERT_HEAD")))
            {
                // revert comes in two flavors, single and sequence
                // test for the existance of a .git/sequencer folder to determine which kind is in-progress
                // Per git core unit test "t/t3510-cherry-pick-sequence.sh", this is always ".git/sequencer/"
                // (and not under ".git/REVERT_HEAD").
                return FileSystem.DirectoryExists(testPath = Path.Combine(gitDirectoryPath, "sequencer"))
                    ? RepositoryCurrentOperation.RevertSequence
                    : RepositoryCurrentOperation.Revert;
            }

            return RepositoryCurrentOperation.None;
        }

        public bool ReadIsIndexCurrentlyLocked()
        {
            string gitDirectoryPath = _repository.GitDirectory;
            string indexLockPath = Path.Combine(gitDirectoryPath, "index.lock");

            return FileSystem.FileExists(indexLockPath);
        }
    }
}
