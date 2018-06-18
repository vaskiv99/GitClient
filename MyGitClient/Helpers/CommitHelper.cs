using MyGitClient.DTO;
using MyGitClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyGitClient.Helpers
{
    public static class CommitHelper
    {
        public static List<CommitDto> CreateCommitsDto(this List<Branch> branches)
        {
            var list = new List<CommitDto>();
            var temp = new CommitDto();
            foreach (var item in branches)
            {
                foreach (var commit in item.Commits)
                {
                    temp.Branch = item.Name;
                    temp.Author = commit.UserEmail;
                    temp.Description = commit.Message;
                    temp.Id = commit.GitCommitId;
                    temp.Time = new DateTime(commit.Time);
                    var addCommit = temp;
                    list.Add(addCommit);
                    temp = new CommitDto();
                }
            }
            return list;
        }
    }
}
