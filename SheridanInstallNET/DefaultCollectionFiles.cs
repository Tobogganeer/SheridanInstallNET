using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SheridanInstallNET
{
    public class DefaultCollectionFiles
    {
        private static readonly string DefaultFileHeader = @"### === {0} - Default Collection === ###
### === {1} === ###

";


        static readonly string Default_Programming = @"Github
Slate
Unity
Visual Studio (Personal Email)
Miro";

        static readonly string Default_SlateUnityGithub = @"Github
Slate
Unity";

        public static void CreateDefault(string directory)
        {
            CreateTemplate(directory, "Programming", "Logs into all tools for programming", Default_Programming);
            CreateTemplate(directory, "Unity Basic", "Logins into Unity, Github and Slate", Default_SlateUnityGithub);
        }

        static void CreateTemplate(string directory, string name, string description, string contents)
        {
            CollectionFile.Create(directory, name, GetDefaultFileText(name, description, contents));
        }

        static string GetDefaultFileText(string name, string description, string contents)
        {
            string header = string.Format(DefaultFileHeader, name, description);
            return header + contents;
        }
    }
}
