using System;

namespace MyGitClient.Helpers
{
    public static class CreateNameForRepositoryHelper
    {
        public static string CreateName(string path)
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
    }
}
