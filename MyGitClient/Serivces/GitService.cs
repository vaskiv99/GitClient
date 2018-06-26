using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MyGitClient.Serivces
{
    public class GitService
    {
        #region Fields
        private string _login = "login";
        private string _password = "password";
        private static string _pathToGit = @"E:\Git\bin\git.exe";
        #endregion

        #region Methods
        public static async Task<GitResult> RunGit(string path, string gitCommand)
        {
            var result = new GitResult();
            await Task.Run(() =>
            {
                ProcessStartInfo procStartInfo = new ProcessStartInfo(_pathToGit, gitCommand)
                {
                    WorkingDirectory = path,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                };
                using (var proc = new Process())
                {
                    proc.StartInfo = procStartInfo;
                    proc.Start();
                    var output = proc.StandardOutput.ReadToEnd();
                    var error = proc.StandardError.ReadToEnd();
                    result.Error = error;
                    result.Output = output;
                    proc.WaitForExit();
                    proc.Close();
                }
            }).ConfigureAwait(false);
            return result;
        }

        public async Task<GitResult> CloneAsync(string url, string path)
        {
            var result = new GitResult();
            await Task.Run(async () =>
            {
                var conifrm = $"{_login}:{_password}@";
                url = url.Insert(8, conifrm);
                var gitCommand = $@"clone {url}";
                result = await RunGit(path, gitCommand).ConfigureAwait(false);
            });
            return result;
        }
        public async Task<GitResult> StageAsync(string path, IEnumerable<string> files)
        {
            var result = new GitResult();
            await Task.Run(async () =>
            {
                var file = string.Join(" ", files);
                var gitCommand = $"add " + file;
                result = await RunGit(path, gitCommand).ConfigureAwait(false);
            }).ConfigureAwait(false);
            return result;
        }
        public async Task<GitResult> StatusAsync(string path)
        {
            var result = new GitResult();
            await Task.Run(async () =>
            {
                var gitCommand = $"status";
                result = await RunGit(path, gitCommand).ConfigureAwait(false);
            }).ConfigureAwait(false);
            return result;
        }
        public async Task<GitResult> CommitAsync(string path, string message)
        {
            var result = new GitResult();
            await Task.Run(async () =>
            {
                var gitCommand = $@"commit -m ""{message}""";
                result = await RunGit(path, gitCommand).ConfigureAwait(false);
            }).ConfigureAwait(false);
            return result;
        }
        public async Task<GitResult> PushAsync(string url, string path)
        {
            var result = new GitResult();
            await Task.Run(async () =>
            {
                var conifrm = $"{_login}:{_password}@";
                url = url.Insert(8, conifrm);
                var gitCommand = $@"push -u {url}";
                result = await RunGit(path, gitCommand).ConfigureAwait(false);
            }).ConfigureAwait(false);
            return result;
        }
        public async Task<GitResult> LastCommitAsync(string path)
        {
            var result = new GitResult();
            await Task.Run(async () =>
            {
                string gitCommand = "log -1";
                result = await RunGit(path, gitCommand);
            });
            return result;
        }
        public async Task<GitResult> GetBranchesAsync(string path)
        {
            var result = new GitResult();
            await Task.Run(async () =>
            {
                var gitCommand = "branch -a";
                result = await RunGit(path, gitCommand);
            });
            return result;
        }
        public async Task<GitResult> GetCurrentBranchAsync(string path)
        {
            var result = new GitResult();
            await Task.Run(async () =>
            {
                var gitCommand = "rev-parse --abbrev-ref HEAD";
                result = await RunGit(path, gitCommand);
                result.Output = result.Output.Trim('\n');
            }).ConfigureAwait(false);
            return result;
        }
        public async Task<GitResult> CreateBranchAsync(string path, string name, bool isCheckout)
        {
            var result = new GitResult();
            await Task.Run(async () =>
            {
                var gitCommand = $"checkout -b {name}";
                var gitCommand2 = $"branch {name}";
                if (isCheckout)
                    result = await RunGit(path, gitCommand);
                result = await RunGit(path, gitCommand2);
            });
            return result;
        }
        public async Task<GitResult> CheckoutAsync(string path, string branchName)
        {
            var result = new GitResult();
            await Task.Run(async () =>
            {
                var gitCommand = $"checkout {branchName}";
                result = await RunGit(path, gitCommand);
            });
            return result;
        }
        public async Task<GitResult> DeleteBranchAsync(string path, string branchName)
        {
            var result = new GitResult();
            await Task.Run(async () =>
            {
                var gitCommand = $"branch -D {branchName}";
                result = await RunGit(path, gitCommand);
            });
            return result;
        }
        public async Task<GitResult> FetchAsync(string url, string path)
        {
            var result = new GitResult();
            await Task.Run(async () =>
            {
                var conifrm = $"{_login}:{_password}@";
                url = url.Insert(8, conifrm);
                var gitCommand = $@"fetch {url}";
                result = await RunGit(path, gitCommand).ConfigureAwait(false);
            }).ConfigureAwait(false);
            return result;
        }
        public async Task<GitResult> PullAsync(string url, string path)
        {
            var result = new GitResult();
            await Task.Run(async () =>
            {
                var conifrm = $"{_login}:{_password}@";
                if (!url.Contains(conifrm))
                {
                    url = url.Insert(8, conifrm);
                }
                var gitCommand = $@"pull {url}";
                result = await RunGit(path, gitCommand).ConfigureAwait(false);
            }).ConfigureAwait(false);
            return result;
        }
        public async Task<GitResult> MergeAsync(string path, string branchName)
        {
            var result = new GitResult();
            await Task.Run(async () =>
            {
                var gitCommand = $"merge {branchName}";
                result = await RunGit(path, gitCommand);
            });
            return result;
        }
        public async Task<GitResult> RemoteAsync(string path)
        {
            var result = new GitResult();
            await Task.Run(async () =>
            {
                var gitCommand = $"remote -v";
                result = await RunGit(path, gitCommand);
            });
            return result;
        }
        public async Task<bool> IsExistPull(string path)
        {
            var result1 = new GitResult();
            var result2 = new GitResult();
            var temp1 = string.Empty;
            var temp2 = string.Empty;
            var branch = await GetCurrentBranchAsync(path).ConfigureAwait(false);
            await Task.Run(async () =>
            {
                var gitCommand1 = "rev-parse HEAD";
                var gitCommand2 = $"rev-parse remotes/origin/{branch.Output}";
                result1 = await RunGit(path, gitCommand1).ConfigureAwait(false);
                result2 = await RunGit(path, gitCommand2).ConfigureAwait(false);
                temp1 = result1.Output.Trim(' ', '\n', '\t');
                temp2 = result2.Output.Trim(' ', '\n', '\t');
            });
            return temp1 == temp2;
        }
        public async Task<GitResult> GetAllCommitsForBranch(string path, string nameBranch)
        {
            var result = new GitResult();
            await Task.Run(async () =>
            {
                var gitCommand = $@"log {nameBranch}";
                result = await RunGit(path, gitCommand).ConfigureAwait(false);
            }).ConfigureAwait(false);
            return result;
        }
        #endregion
    }
}

