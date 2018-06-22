using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyGitClient.Models;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MyGitClient.MongoContext;

namespace MyGitClient.Serivces
{
    public class BranchService
    {
        private RepositoriesService _repositoryService;
        private MongoDbContext _context;
        public BranchService()
        {
            _repositoryService = new RepositoriesService();
            _context = new MongoDbContext();
        }

        public async Task<IEnumerable<Models.Branch>> GetBranchFromRepository(string url)
        {
            var branches = new List<Models.Branch>();
            await Task.Run(() =>
            {
                using (var repo = new LibGit2Sharp.Repository(url))
                {
                    foreach (LibGit2Sharp.Branch b in repo.Branches.Where(b => !b.IsRemote))
                    {
                        var branch = new Models.Branch() { Id = Guid.NewGuid(), Name = b.FriendlyName };
                        branches.Add(branch);
                    }
                }
            });
            return branches;
        }
        public async Task<List<Branch>> GetBranchFromRepositoryAsync(Guid repositoryId)
        {
            var repository = await _repositoryService.GetRepositoryAsync(repositoryId).ConfigureAwait(false);
            return repository.Branches;
        }
        public List<Branch> GetBranches(Guid repositoryId)
        {
            var repository = _repositoryService.GetRepository(repositoryId);
            return repository.Branches;
        }
        public async Task<Branch> GetBranchFromRepositoryAsync(Guid repositoryId, Guid branchId)
        {
            var repo = await _repositoryService.GetRepositoryAsync(repositoryId).ConfigureAwait(false);
            var branch = repo.Branches.AsQueryable().FirstOrDefault(b => b.Id == branchId);
            return branch;
        }
        public async Task<bool> DeleteBranchFromRepositoryAsync(Guid repositoryId, Guid branchId)
        {
            var branch = await GetBranchFromRepositoryAsync(repositoryId, branchId).ConfigureAwait(false);
            var delete = Builders<Repository>.Update.Pull(u => u.Branches, branch);
            var result = await _context.Repositories.UpdateOneAsync(u => u.Id == repositoryId, delete).ConfigureAwait(false);
            return result.IsAcknowledged;
        }
        public async Task AddBranchToRepositoryAsync(Guid repositoryId, Branch branch)
        {
            var repo = await _repositoryService.GetRepositoryAsync(repositoryId).ConfigureAwait(false);
            repo.Branches.Add(branch);
            await _context.Repositories.ReplaceOneAsync(r => r.Id == repositoryId, repo);
        }
        public async Task<Guid> GetBranchIdAsync(Guid repositoryId, string name)
        {
            var branches = await GetBranchFromRepositoryAsync(repositoryId).ConfigureAwait(false);
            var branch = branches.FirstOrDefault(b => b.Name == name);
            return branch.Id;
        }
    }
}
