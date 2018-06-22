using System;
using System.Collections.Generic;

namespace MyGitClient.Models
{
    public class Repository
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Url { get; set; }
        public List<Models.Branch> Branches { get; set; }
        = new List<Models.Branch>();
    }
}
