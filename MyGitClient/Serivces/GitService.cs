using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MyGitClient.Serivces
{
    public class GitService
    {
        private string _login = "YourLoginGitHub";
        private string _password = "YourPasswordGitHub";
        private static string _pathToGit = @"E:\Git\bin\git.exe";
        public async static Task<GitResult> RunGit(string path, string gitCommand)
        {
            var result = new GitResult();
            await Task.Run(() =>
            {
                ProcessStartInfo procStartInfo = new ProcessStartInfo(_pathToGit, gitCommand);
                procStartInfo.WorkingDirectory = path;
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.RedirectStandardError = true;
                procStartInfo.UseShellExecute = false;
                procStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                procStartInfo.CreateNoWindow = true;
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
                string conifrm = $"{_login}:{_password}@";
                url = url.Insert(8, conifrm);
                string gitCommand = $@"clone {url}";
                result = await RunGit(path, gitCommand).ConfigureAwait(false);
            });
            return result;
        }
        public async Task<GitResult> StageAsync(string path, IEnumerable<string> files)
        {
            var result = new GitResult();
            await Task.Run(async () =>
            {
                string file = string.Join(" ", files);
                string gitCommand = $"add " + file;
                result = await RunGit(path, gitCommand).ConfigureAwait(false);
            }).ConfigureAwait(false);
            return result;
        }
        public async Task<GitResult> StatusAsync(string path)
        {
            var result = new GitResult();
            await Task.Run(async () =>
            {
                string gitCommand = $"status";
                result = await RunGit(path, gitCommand).ConfigureAwait(false);
            }).ConfigureAwait(false);
            return result;
        }
        public async Task<GitResult> CommitAsync(string path, string message)
        {
            var result = new GitResult();
            await Task.Run(async () =>
            {
                string gitCommand = $@"commit -m ""{message}""";
                result = await RunGit(path, gitCommand).ConfigureAwait(false);
            }).ConfigureAwait(false);
            return result;
        }
        public async Task<GitResult> PushAsync(string url, string path)
        {
            var result = new GitResult();
            await Task.Run(async () =>
            {
                string conifrm = $"{_login}:{_password}@";
                url = url.Insert(8, conifrm);
                string gitCommand = $@"push {url} --all";
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
                string gitCommand = "branch -a";
                result = await RunGit(path, gitCommand);
            });
            return result;
        }
        public async Task<GitResult> GetCurrentBranchAsync(string path)
        {
            var result = new GitResult();
            await Task.Run(async () =>
            {
                string gitCommand = "rev-parse --abbrev-ref HEAD";
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
                string gitCommand = $"checkout -b {name}";
                string gitCommand2 = $"branch {name}";
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
                string gitCommand = $"checkout {branchName}";
                result = await RunGit(path, gitCommand);
            });
            return result;
        }
        public async Task<GitResult> DeleteBranchAsync(string path, string branchName)
        {
            var result = new GitResult();
            await Task.Run(async () =>
            {
                string gitCommand = $"branch -D {branchName}";
                result = await RunGit(path, gitCommand);
            });
            return result;
        }
        public async Task<GitResult> FetchAsync(string url, string path)
        {
            var result = new GitResult();
            await Task.Run(async () =>
            {
                string conifrm = $"{_login}:{_password}@";
                url = url.Insert(8, conifrm);
                string gitCommand = $@"fetch {url}";
                result = await RunGit(path, gitCommand).ConfigureAwait(false);
            }).ConfigureAwait(false);
            return result;
        }
        public async Task<GitResult> PullAsync(string url, string path)
        {
            var result = new GitResult();
            await Task.Run(async () =>
            {
                string conifrm = $"{_login}:{_password}@";
                url = url.Insert(8, conifrm);
                string gitCommand = $@"pull {url}";
                result = await RunGit(path, gitCommand).ConfigureAwait(false);
            }).ConfigureAwait(false);
            return result;
        }
        public async Task<GitResult> MergeAsync(string path, string branchName)
        {
            var result = new GitResult();
            await Task.Run(async () =>
            {
                string gitCommand = $"merge {branchName}";
                result = await RunGit(path, gitCommand);
            });
            return result;
        }
    }
}

