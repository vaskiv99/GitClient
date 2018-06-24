using MyGitClient.DTO;
using MyGitClient.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyGitClient.Serivces
{
    public class GitManager
    {
        private RepositoriesService _repositoriesService;
        private BranchService _branchService;
        private CommitService _commitService;
        private GitService _gitService;

        public GitManager()
        {
            _repositoriesService = new RepositoriesService();
            _branchService = new BranchService();
            _commitService = new CommitService();
            _gitService = new GitService();
        }

        public async Task<Repository> CloneAsync(string url, string path, string name)
        {
            Repository repository = new Repository();
            await Task.Run(async () =>
            {
                await _gitService.CloneAsync(url, path).ConfigureAwait(false);
                path = path + $@"\{name}";
                var branches = await _gitService.GetBranchesAsync(path);
                var parseBranches = await GitParser.ParseBranchAsync(branches.Output);
                repository = new Repository()
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Path = path,
                    Url = url,
                    Branches = parseBranches
                };
                await _repositoriesService.AddRepositoryAsync(repository).ConfigureAwait(false);
            }).ConfigureAwait(false);
            return repository;
        }
        public async Task<CommitDto> GitCommitAsync(string message, Guid repositoryId, IEnumerable<string> files)
        {
            CommitDto commitDto = new CommitDto();
            await Task.Run(async () =>
             {
                 var repository = await _repositoriesService.GetRepositoryAsync(repositoryId).ConfigureAwait(false);
                 var branch = await _gitService.GetCurrentBranchAsync(repository.Path).ConfigureAwait(false);
                 var branchId = await _branchService.GetBranchIdAsync(repositoryId, branch.Output).ConfigureAwait(false);
                 await _gitService.StageAsync(repository.Path, files).ConfigureAwait(false);
                 var commit = await _gitService.CommitAsync(repository.Path, message).ConfigureAwait(false);
                 var lastCommit = await _gitService.LastCommitAsync(repository.Path).ConfigureAwait(false);
                 var addCommit = new Models.Commit()
                 {
                     Id = Guid.NewGuid(),
                     Message = message,
                     GitCommitId = await GitParser.ParseCommitIdAsync(lastCommit.Output).ConfigureAwait(false),
                     UserEmail = await GitParser.ParseCommitAuthorAsync(lastCommit.Output).ConfigureAwait(false),
                     Time = DateTime.Now.Ticks
                 };
                 await _commitService.AddCommitAsync(repositoryId, branchId, addCommit).ConfigureAwait(false);
                 commitDto.Branch = branch.Output;
                 commitDto.Author = addCommit.UserEmail;
                 commitDto.Description = addCommit.Message;
                 commitDto.Time = new DateTime(addCommit.Time);
                 commitDto.Id = addCommit.GitCommitId;
             }).ConfigureAwait(false);
            return commitDto;
        }
        public async Task<List<string>> GitStatusAsync(Guid repositoryId)
        {
            var list = new List<string>();
            await Task.Run(async () =>
             {
                 var repository = await _repositoriesService.GetRepositoryAsync(repositoryId).ConfigureAwait(false);
                 var status = await _gitService.StatusAsync(repository.Path);
                 list = await GitParser.ParseStatusAsync(status.Output);
             }).ConfigureAwait(false);
            return list;
        }
        public async Task GitPushAsync(Guid repositoryId)
        {
            await Task.Run(async () =>
           {
               var repository = await _repositoriesService.GetRepositoryAsync(repositoryId).ConfigureAwait(false);
               await _gitService.PushAsync(repository.Url, repository.Path);
           }).ConfigureAwait(false);
        }
        public async Task<Branch> GitCreateBranchAsync(Guid repositoryId, bool isCheckOut, string name)
        {
            var branch = new Branch();
            await Task.Run(async () =>
            {
                var repository = await _repositoriesService.GetRepositoryAsync(repositoryId).ConfigureAwait(false);
                await _gitService.CreateBranchAsync(repository.Path, name, isCheckOut);
                branch.Id = Guid.NewGuid();
                branch.Name = name;
                await _branchService.AddBranchToRepositoryAsync(repositoryId, branch);
            });
            return branch;
        }
        public async Task GitCheckoutAsync(Guid repositoryId, Guid branchId)
        {
            await Task.Run(async () =>
            {
                var repository = await _repositoriesService.GetRepositoryAsync(repositoryId).ConfigureAwait(false);
                var branch = await _branchService.GetBranchFromRepositoryAsync(repositoryId, branchId).ConfigureAwait(false);
                await _gitService.CheckoutAsync(repository.Path, branch.Name);
            });
        }
        public async Task GitDeleteBranchAsync(Guid repositoryId, Guid branchId)
        {
            await Task.Run(async () =>
            {
                var repository = await _repositoriesService.GetRepositoryAsync(repositoryId).ConfigureAwait(false);
                await _gitService.CheckoutAsync(repository.Path, "master");
                var branch = await _branchService.GetBranchFromRepositoryAsync(repositoryId, branchId).ConfigureAwait(false);
                await _gitService.DeleteBranchAsync(repository.Path, branch.Name);
                await _branchService.DeleteBranchFromRepositoryAsync(repositoryId, branchId).ConfigureAwait(false);
            });
        }
        public async Task<bool> GitFetchAsync(Guid repositoryId)
        {
            var repository = await _repositoriesService.GetRepositoryAsync(repositoryId).ConfigureAwait(false);
            var fetch = await _gitService.FetchAsync(repository.Url, repository.Path).ConfigureAwait(false);
            var result = await GitParser.ParseFetchAsync(fetch.Output).ConfigureAwait(false);
            return result;
        }
        public async Task<Tuple<string, bool>> GitPullAsync(Guid repositoryId)
        {
            var message = "Successful";
            var isSuccess = true;
            var repository = await _repositoriesService.GetRepositoryAsync(repositoryId).ConfigureAwait(false);
            var pull = await _gitService.PullAsync(repository.Url, repository.Path).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(pull.Error))
            {
                message = pull.Error;
                isSuccess = false;
            }
            var result = Tuple.Create(message, isSuccess);
            return result;
        }
        public async Task<Tuple<string, bool>> GitMerge(Guid repositoryId, Guid branchId)
        {
            var message = "Successful";
            var isSuccess = true;
            var repository = await _repositoriesService.GetRepositoryAsync(repositoryId).ConfigureAwait(false);
            var branch = await _branchService.GetBranchFromRepositoryAsync(repositoryId, branchId).ConfigureAwait(false);
            var merge = await _gitService.MergeAsync(repository.Path, branch.Name).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(merge.Error) && merge.Error.Contains("Fatal"))
            {
                message = merge.Error;
                isSuccess = false;
            }
            else
            {
                var name = await NameHeadBranch(repositoryId).ConfigureAwait(false);
                var description = $"Merge branch({branch.Name}) into branch({name})";
                var files = await GitStatusAsync(repositoryId).ConfigureAwait(false);
                var commit = await GitCommitAsync(description, repositoryId, files);
            }
            var result = Tuple.Create(message, isSuccess);
            return result;
        }
        public async Task<string> NameHeadBranch(Guid repositoryId)
        {
            var repository = await _repositoriesService.GetRepositoryAsync(repositoryId).ConfigureAwait(false);
            var branchName = await _gitService.GetCurrentBranchAsync(repository.Path).ConfigureAwait(false);
            return branchName.Output;
        }
        public async Task<Repository> AddExistingRepositoryAsync(string path)
        {
            var branches = await _gitService.GetBranchesAsync(path).ConfigureAwait(false);
            var parseBranches =await GitParser.ParseBranchAsync(branches.Output).ConfigureAwait(false);
            var createName = path.Split('\\');
            var name = createName[createName.Length-1].Trim();
            var remote = await _gitService.RemoteAsync(path).ConfigureAwait(false);
            var url = await GitParser.ParseRemoteAsync(remote.Output).ConfigureAwait(false);
            var repository = new Repository()
            {
                Id = Guid.NewGuid(),
                Name = name,
                Path = path,
                Branches = parseBranches,
                Url = url
            };
            await _repositoriesService.AddRepositoryAsync(repository).ConfigureAwait(false);
            return repository;
        }
        public async Task<bool> IsExistPullAsync(Guid repositoryId)
        {
            var repository = await _repositoriesService.GetRepositoryAsync(repositoryId).ConfigureAwait(false);
            var result = await _gitService.IsExistPull(repository.Path).ConfigureAwait(false);
            return result;
        }
    }
}
