using MyGitClient.MongoContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyGitClient.Models;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace MyGitClient.Serivces
{
    public class RepositoriesService
    {
        private MongoDbContext _context;

        public RepositoriesService()
        {
            _context = new MongoDbContext();
        }

        public async Task AddRepositoryAsync(Repository repository)
        {
            await _context.Repositories.InsertOneAsync(repository);
        }
        public async Task DeleteRepositoryAsync(Guid id)
        {
            await _context.Repositories.DeleteOneAsync(r => r.Id == id);
        }
        public async Task<Repository> GetRepositoryAsync(Guid id)
        {
            var repository = await _context.Repositories.AsQueryable().FirstOrDefaultAsync(r => r.Id == id);
            return repository;
        }
        public async Task<List<Repository>> GetRepositoriesAsync()
        {
            var repositories = await _context.Repositories.AsQueryable().ToListAsync();
            return repositories;
        }
        public List<Repository> GetRepositories()
        {
            var repositories = _context.Repositories.Find(_ => true).ToList();
            return repositories;
        }
        public Repository GetRepository(Guid repositoryId)
        {
            var repository = _context.Repositories.AsQueryable().FirstOrDefault(r => r.Id == repositoryId);
            return repository;
        }

    }
}
