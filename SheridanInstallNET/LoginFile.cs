using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SheridanInstallNET
{
    public class LoginFile
    {
        public List<string> Lines { get; private set; }
        public int CurrentLine { get; private set; }

        private static readonly string Extension = ".login";

        public static LoginFile Load(string relativePath)
        {

        }

        public static LoginFile[] LoadAll(string directory)
        {
            directory = Path.Combine(Program.RootDirectory, directory);

            if (!Directory.Exists(directory))
            {

            }
        }
    }
}
