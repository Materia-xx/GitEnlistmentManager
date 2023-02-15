//using GitEnlistmentManager.Extensions;
//using System.Collections.Generic;
//using System.Threading.Tasks;

//namespace GitEnlistmentManager.DTOs
//{
//    public class GemNodeContextSet
//    {
//        public RepoCollection? RepoCollection => OverrideNodeContext.RepoCollection ?? BaseNodeContext.RepoCollection;

//        public Repo? Repo => OverrideNodeContext.Repo ?? BaseNodeContext.Repo;

//        public Bucket? Bucket => OverrideNodeContext.Bucket ?? BaseNodeContext.Bucket;

//        public Enlistment? Enlistment => OverrideNodeContext.Enlistment ?? BaseNodeContext.Enlistment;

//        /// <summary>
//        /// Base node context gets set on every command by the system every time a command is run
//        /// according to what is selected in the UI or what directory you in with a command prompt
//        /// </summary>
//        public GemNodeContext BaseNodeContext { get; } = new GemNodeContext();


//        /// <summary>
//        /// Override node context is something that would only be set by the code that is making up
//        /// an in-line command set. These values are not reset or changed by the command set executor.
//        /// </summary>
//        public GemNodeContext OverrideNodeContext { get; } = new GemNodeContext();

//        public string? GetWorkingDirectory()
//        {
//            return Enlistment?.GetDirectoryInfo()?.FullName ?? Bucket?.GetDirectoryInfo()?.FullName ?? Repo?.GetDirectoryInfo()?.FullName ?? RepoCollection?.RepoCollectionDirectoryPath;
//        }

//        public async Task<Dictionary<string, string>> GetTokens()
//        {
//            if (this.Enlistment != null)
//            {
//                return await this.Enlistment.GetTokens().ConfigureAwait(false);
//            }

//            if (this.Bucket != null)
//            {
//                return this.Bucket.GetTokens();
//            }

//            if (this.Repo != null)
//            {
//                return this.Repo.GetTokens();
//            }

//            if (this.RepoCollection != null)
//            {
//                return this.RepoCollection.GetTokens();
//            }

//            return new Dictionary<string, string>();
//        }

//    }
//}
