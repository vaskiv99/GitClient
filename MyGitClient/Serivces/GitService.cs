using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using MyGitClient.DTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MyGitClient.Serivces
{
    public class GitService
    {
        private RepositoriesService _repositoriesService;
        private BranchService _branchService;
        private CommitService _commitService;
        private string _login;
        private string _password;
        public GitService()
        {
            _repositoriesService = new RepositoriesService();
            _branchService = new BranchService();
            _commitService = new CommitService();
        }

        public async Task<Models.Repository> CloneAsync(string url, string path, string name)
        {
            Models.Repository repository = new Models.Repository();
            await Task.Run(async () =>
            {
                if (!Directory.Exists(path))
                {
                    var directory = Directory.CreateDirectory(path);
                }
                Repository.Clone(url, path);
                var branches = await _branchService.GetBranchFromRepository(path).ConfigureAwait(false);
                repository = new Models.Repository()
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Path = path,
                    Branches = branches.ToList()
                };
                await _repositoriesService.AddRepositoryAsync(repository).ConfigureAwait(false);
            }).ConfigureAwait(false);
            return repository;
        }

        public string CreateName(string path)
        {
            var name = string.Empty;
            for (int i = path.Length - 5; i > 0; i--)
            {
                if (path[i] == '/')
                    break;
                name += path[i].ToString();
            }
            var temp = name.ToCharArray();
            Array.Reverse(temp);
            return new string(temp);
        }

        public async Task<CommitDto> GitCommitAsync(string message, Guid repositoryId, IEnumerable<string> files)
        {
            CommitDto commitDto = new CommitDto();
            await Task.Run(async () =>
             {
                 var repository = await _repositoriesService.GetRepositoryAsync(repositoryId).ConfigureAwait(false);
                 DirectoryInfo directoryInfo = new DirectoryInfo(repository.Path);
                 using (var repo = new Repository(directoryInfo.FullName))
                 {
                     var branch = repo.Head;
                     var branchId = await _branchService.GetBranchIdAsync(repositoryId, branch.FriendlyName).ConfigureAwait(false);
                     Commands.Stage(repo, files);

                     var commit = repo.Commit(message, new Signature("vaskiv99", "vaskiv99@ukr.net", DateTimeOffset.Now),
                     new Signature("vaskiv99", "vaskiv99@ukr.net", DateTimeOffset.Now), new CommitOptions() { AllowEmptyCommit = false });
                     var addCommit = new Models.Commit()
                     {
                         Id = Guid.NewGuid(),
                         Message = message,
                         GitCommitId = commit.Id.ToString(),
                         UserEmail = commit.Committer.Email,
                         Time = DateTime.Now.Ticks
                     };
                     await _commitService.AddCommitAsync(repositoryId, branchId, addCommit).ConfigureAwait(false);
                     commitDto.Branch = branch.FriendlyName;
                     commitDto.Author = addCommit.UserEmail;
                     commitDto.Description = addCommit.Message;
                     commitDto.Time = new DateTime(addCommit.Time);
                     commitDto.Id = addCommit.GitCommitId;
                 }
             }).ConfigureAwait(false);
            return commitDto;
        }

        public async Task<List<string>> GitStatusAsync(Guid repositoryId)
        {
            var list = new List<string>();
            await Task.Run(async () =>
             {
                 var repository = await _repositoriesService.GetRepositoryAsync(repositoryId).ConfigureAwait(false);
                 using (var repo = new Repository(repository.Path))
                 {
                     foreach (var item in repo.RetrieveStatus(new StatusOptions()))
                     {
                         if (item.State == FileStatus.ModifiedInWorkdir)
                             list.Add(item.FilePath);
                     }
                 }
             }).ConfigureAwait(false);
            return list;
        }

        public async Task GitPushAsync(Guid repositoryId)
        {
            await Task.Run(async () =>
           {

               var repository = await _repositoriesService.GetRepositoryAsync(repositoryId).ConfigureAwait(false);
               using (var repo = new Repository(repository.Path))
               {
                   var branch = repo.Head;
                   PushOptions options = new PushOptions();
                   options.CredentialsProvider = new CredentialsHandler(
                       (url, usernameFromUrl, types) =>
                           new UsernamePasswordCredentials()
                           {
                               Username = _login,
                               Password = _password
                           });
                   repo.Network.Push(repo.Branches[branch.FriendlyName], options);
               }
           }).ConfigureAwait(false);
        }

        //public async Task GitPullAsync(Guid repositoryId)
        //{
        //    await Task.Run(async () =>
        //   {
        //       var repository = await _repositoriesService.GetRepositoryAsync(repositoryId).ConfigureAwait(false);
        //       using (var repo = new Repository(repository.Path))
        //       {

        //           PullOptions options = new PullOptions();
        //           options.FetchOptions = new FetchOptions();
        //           options.FetchOptions.CredentialsProvider = new CredentialsHandler(
        //                   (url, usernameFromUrl, types) =>
        //                   new UsernamePasswordCredentials()
        //                   {
        //                       Username = _login,
        //                       Password = _password
        //                   });
        //           repo.Network.Pull(new Signature("vaskiv99", "vaskiv99@ukr.net", new DateTimeOffset(DateTime.Now)), options);
        //       }
        //   });
        //}

        public async Task<string> GitFetchAsync(Guid repositoryId)
        {
            var repository = await _repositoriesService.GetRepositoryAsync(repositoryId).ConfigureAwait(false);
            string result = string.Empty;
            await Task.Run(() =>
            {
                using (var repo = new Repository(repository.Path))
                {
                    FetchOptions options = new FetchOptions();
                    options.CredentialsProvider = new CredentialsHandler((url, usernameFromUrl, types) =>
                        new UsernamePasswordCredentials()
                        {
                            Username = _login,
                            Password = _password
                        });
                    foreach (Remote remote in repo.Network.Remotes)
                    {
                        IEnumerable<string> refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
                        Commands.Fetch(repo, remote.Name, refSpecs, options, result);
                    }
                }
            }).ConfigureAwait(false);
            return result;
        }

        public async Task GitCheckOutAsync(Guid repositoryId, Guid branchId)
        {
            var repository = await _repositoriesService.GetRepositoryAsync(repositoryId).ConfigureAwait(false);
            var branchFromRepository = await _branchService.GetBranchFromRepositoryAsync(repositoryId, branchId).ConfigureAwait(false);
            using (var repo = new Repository(repository.Path))
            {
                var branch = repo.Branches[branchFromRepository.Name];
                if (branch == null)
                    return;

                Branch currentBranch = Commands.Checkout(repo, branch);
            }

        }

        public async Task<Models.Repository> GitInitRepositoryAsync(string path,string name)
        {
            var repository = new Models.Repository();
            await Task.Run(() =>
            {
                var repo = Repository.Init(path);
                repository.Id = Guid.NewGuid();
                repository.Name = name;
                repository.Path = path;
            }).ConfigureAwait(false);
            return repository;
        }

        public async Task GitAddFileToRepository(Guid repositoryId, string path)
        {
            var repository = await _repositoriesService.GetRepositoryAsync(repositoryId).ConfigureAwait(false);
            await Task.Run(() =>
            {
                using (var repo = new Repository(repository.Path))
                {
                    repo.Index.Add(path);
                }
            });
        }

    }
}
