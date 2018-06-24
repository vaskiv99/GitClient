using System;

namespace MyGitClient.Models
{
    public class Merge
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public Guid FromBranchId { get; set; }
        public Guid IntoBranchId { get; set; }
        public long Time { get; set; }
        public string Description { get; set; }
    }
}
