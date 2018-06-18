using System;
using System.Collections.Generic;

namespace MyGitClient.Models
{
    public class Branch
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<Models.Commit> Commits { get; set; }
               = new List<Models.Commit>();
    }
}
