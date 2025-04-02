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
        public string Name;
        //public string Category;
        public List<string> Lines;

        public int Order;
        public int CurrentLine { get; private set; }
        public bool EnabledByDefault;

        public bool enabled;

        private static readonly string Extension = "login";
        //private static readonly string CategoryKey = "Category";
        private static readonly string OrderKey = "Order";
        private static readonly string EnabledByDefaultKey = "EnabledByDefault";


        public LoginFile(string[] lines, string path)
        {
            Name = Path.GetFileNameWithoutExtension(path);
            Lines = new List<string>(lines);

            //INIParser.TryGetValue(Lines, CategoryKey, out Category);
            INIParser.TryGetInt(Lines, OrderKey, out Order, -1);
            INIParser.TryGetBool(Lines, EnabledByDefaultKey, out EnabledByDefault, true);

            enabled = EnabledByDefault;
        }


        public static LoginFile Load(string path)
        {
            if (File.Exists(path))
            {
                string[] file = File.ReadAllLines(path);
                return new LoginFile(file, path);
            }
            return null;
        }

        public static List<LoginFile> LoadAll(string directory)
        {
            directory = Path.Combine(Program.RootDirectory, directory);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                InOut.WriteLine($"{directory} not found.");
                return new List<LoginFile>();
            }

            List<LoginFile> files = new List<LoginFile>();

            foreach (string filePath in Directory.EnumerateFiles(directory, $"*.{Extension}"))
            {
                LoginFile file = Load(filePath);
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

    /*
    public class LoginCategory
    {
        public string name;
        public List<LoginFile> files;
        public bool AllDisabled => files.All((file) => !file.enabled);

        public LoginCategory(string name, List<LoginFile> files)
        {
            this.name = name;
            this.files = files;
        }

        /// <summary>
        /// Takes in unsorted files and returns them, sorted by order. Categories are sorted alphabetically.
        /// </summary>
        /// <param name="unsortedFiles">A list of unsorted files, typically loaded from disk</param>
        /// <returns></returns>
        public static List<LoginCategory> GetCategories(List<LoginFile> unsortedFiles)
        {
            // Return empty list of categories
            if (unsortedFiles.Count == 0)
                return new List<LoginCategory>();

            // Create dictionary for easy adding
            Dictionary<string, LoginCategory> categories = new Dictionary<string, LoginCategory>
            {
                { string.Empty, new LoginCategory(string.Empty, new List<LoginFile>()) }
            };

            // Add files to corresponding categories
            foreach (LoginFile file in unsortedFiles)
            {
                string category = string.IsNullOrWhiteSpace(file.Category) ? string.Empty : file.Category;

                // Check if we have a collection of this category yet
                if (!categories.ContainsKey(category))
                    categories.Add(category, new LoginCategory(category, new List<LoginFile>()));

                categories[category].files.Add(file);
            }

            // Sort each individual category by order
            foreach (LoginCategory category in categories.Values)
            {
                
                category.files.Sort((a, b) => a.Order.CompareTo(b.Order));
            }

            // Sort the categories themselves alphabetically
            List<LoginCategory> categoryList = categories.Values.ToList();
            categoryList.Sort((a, b) => a.name.CompareTo(b.name));
            return categoryList;
        }
    }
    */
}
