using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyGitClient.DTO
{
    public class CommitDto
    {
        public string Id { get; set; }
        public DateTime Time { get; set; }
        public string Description { get; set; }
        public string Branch { get; set; }
        public string Author { get; set; }
    }
}
