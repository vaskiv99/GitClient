using System;

namespace MyGitClient.Models
{
    public class Commit
    {
        public Guid Id { get; set; }
        public string GitCommitId { get; set; }
        public string UserEmail { get; set; }
        public long Time { get; set; }
        public string Message { get; set; }
    }
}
