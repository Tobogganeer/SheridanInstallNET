using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SheridanInstallNET
{
    public class CollectionFile
    {
        public string Name;
        public List<LoginFile> Services;

        private static readonly string Extension = "coll";


        public CollectionFile(string[] lines, string path)
        {
            Name = Path.GetFileNameWithoutExtension(path);
            Services = new List<LoginFile>(lines.Length);

            // Loop through each string
            foreach (string login in lines)
            {
                string loginName = login.ToLower().Trim();
                // Check if there is a matching file loaded
                foreach (LoginFile file in Program.LoginFiles)
                {
                    if (!Services.Contains(file) && file.Name.ToLower() == loginName)
                        Services.Add(file);
                }
            }
        }


        public static CollectionFile Load(string path)
        {
            if (File.Exists(path))
            {
                string[] file = File.ReadAllLines(path);
                return new CollectionFile(file, path);
            }
            return null;
        }

        public static List<CollectionFile> LoadAll(string directory)
        {
            directory = Path.Combine(Program.RootDirectory, directory);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                InOut.WriteLine($"{directory} not found.");
                return new List<CollectionFile>();
            }

            List<CollectionFile> files = new List<CollectionFile>();

            foreach (string filePath in Directory.EnumerateFiles(directory, $"*.{Extension}"))
            {
                CollectionFile file = Load(filePath);
                if (file != null)
                    files.Add(file);
            }

            return files;
        }

        public static void Create(string directory, string name, string contents)
        {
            directory = Path.Combine(Program.RootDirectory, directory);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(Path.Combine(directory, name + "." + Extension), contents);
        }
    }
}
