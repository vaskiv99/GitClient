using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyGitClient.Models;
using MongoDB.Driver;
using MyGitClient.MongoContext;

namespace MyGitClient.Serivces
{
    class CommitService
    {
        private RepositoriesService _repositoriesService;
        private BranchService _branchService;
        private MongoDbContext _context;

        public CommitService()
        {
            _repositoriesService = new RepositoriesService();
            _branchService = new BranchService();
            _context = new MongoDbContext();
        }

        public async Task AddCommitAsync(Guid repositoryId, Guid branchId, Models.Commit commit)
        {
            var branches = await _branchService.GetBranchFromRepositoryAsync(repositoryId);
            branches.FirstOrDefault(b => b.Id == branchId).Commits.Add(commit);
            var repo = await _repositoriesService.GetRepositoryAsync(repositoryId).ConfigureAwait(false);
            repo.Branches = branches;
            await _context.Repositories.ReplaceOneAsync(r => r.Id == repositoryId, repo);
        }

        public async Task<bool> DeleteCommitAsync(Guid repositoryId, Guid branchId, Guid commitId)
        {
            var branches = await _branchService.GetBranchFromRepositoryAsync(repositoryId);
            var commit =await GetCommitFromBranchAsync(repositoryId, branchId, commitId).ConfigureAwait(false);
            branches.FirstOrDefault(b => b.Id == branchId).Commits.Remove(commit);
            var repo = await _repositoriesService.GetRepositoryAsync(repositoryId).ConfigureAwait(false);
            repo.Branches = branches;
            var result = await _context.Repositories.ReplaceOneAsync(r => r.Id == repositoryId, repo);
            return result.IsAcknowledged;
        }

        public async Task<Commit> GetCommitFromBranchAsync(Guid repositoryId, Guid branchId, Guid commitId)
        {
            var branch = await _branchService.GetBranchFromRepositoryAsync(repositoryId, branchId).ConfigureAwait(false);
            var commit = branch.Commits.AsEnumerable().FirstOrDefault(c => c.Id == commitId);
            return commit;
        }

        public async Task<IList<Commit>> GetCommitsFromBranchAsync(Guid repositoryId, Guid branchId)
        {
            var branch = await _branchService.GetBranchFromRepositoryAsync(repositoryId, branchId).ConfigureAwait(false);
            return branch.Commits;
        }

        public List<Commit> GetCommitsFromRepository(Guid repositoryId)
        {
            var list = new List<Commit>();
            var repo =  _repositoriesService.GetRepository(repositoryId);
            foreach (var item in repo.Branches)
            {
                list.AddRange(item.Commits);
            }
            return list;
        }
    }
}
