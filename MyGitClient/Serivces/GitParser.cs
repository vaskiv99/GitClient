using MyGitClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MyGitClient.Serivces
{
    public static class GitParser
    {
        public async static Task<List<string>> ParseStatusAsync(string status)
        {
            var list = new List<string>();
            await Task.Run(() =>
            {
                var regex = new Regex(@"modified:\s*(?<value>.+)$", RegexOptions.Multiline);
                var matches = regex.Matches(status);
                foreach (Match item in matches)
                {
                    var str = item.Value.Split(':');
                    list.Add(str[1].TrimStart());
                }
            });
            return list;
        }
        public async static Task<string> ParseCommitIdAsync(string commit)
        {
            var Id = string.Empty;
            await Task.Run(() =>
            {
                var regex = new Regex(@"commit\s*(?<value>.+)$", RegexOptions.Multiline);
                var matches = regex.Matches(commit);
                foreach (Match item in matches)
                {
                    var str = item.Value.Split(' ');
                    Id = str[1].TrimStart();
                }
            });
            return Id;
        }
        public async static Task<string> ParseCommitAuthorAsync(string commit)
        {
            var author = string.Empty;
            await Task.Run(() =>
            {
                var regex = new Regex(@"Author:\s*(?<value>.+)$", RegexOptions.Multiline);
                var matches = regex.Matches(commit);
                foreach (Match item in matches)
                {
                    var str = item.Value.Split(':');
                    author = str[1].TrimStart();
                }
            });
            return author;
        }
        public async static Task<List<Branch>> ParseBranchAsync(string branch)
        {
            branch = branch.Trim();
            var listNames = new List<string>();
            var listBranches = new List<Branch>();
            var tempBranch = new Branch();
            await Task.Run(() =>
            {
                var str = branch.Split('\n');
                for (int i = 0; i < str.Length; i++)
                {
                    var br = str[i].Trim(new char[] { ' ', '*' }).Split('/');
                    str[i] = br[br.Length - 1];
                    listNames.Add(str[i]);
                }
                var temp = listNames.Distinct();
                foreach (var item in temp)
                {
                    tempBranch.Id = Guid.NewGuid();
                    tempBranch.Name = item;
                    var addedBranch = tempBranch;
                    listBranches.Add(addedBranch);
                    tempBranch = new Branch();
                }
            });
            return listBranches;
        }
        public async static Task<bool> ParseFetchAsync(string fetch)
        {
            var result = true;
            await Task.Run(() =>
            {
                if (!fetch.Contains("remote"))
                    result = false;
            });
            return result;
        }
        public async static Task<string> ParseRemoteAsync(string remote)
        {
            var result = string.Empty;
            await Task.Run(() =>
            {
                var regex = new Regex(@"origin\s*(?<value>.+)$", RegexOptions.Multiline);
                var matches = regex.Matches(remote);
                foreach (Match item in matches)
                {
                    var str = item.Value.Split(' ','\t');
                    result = str[1].TrimStart();
                }
            });
            return result;
        }
    }
}
