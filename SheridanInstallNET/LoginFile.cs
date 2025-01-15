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
        public string Category;
        public List<string> Lines;

        public int Order;
        public int CurrentLine { get; private set; }

        private static readonly string Extension = "login";
        private static readonly string CategoryKey = "Category";
        private static readonly string OrderKey = "Order";

        private static readonly string BlankFile = @"
### Config
# Category is used to group files for easy enabling/disabling
Category=
# Lower orders are loaded first
Order=


### Commands
# type [text] - types the given text
# typeenter [text] - types and then hits enter
# enter - hits enter
# tab - hits tab (useful for navigating between controls)
# tab [amount] - hits tab a bunch of times
# tabenter - hits tab and then enter

# open [program] - opens the given program
# wait [seconds] - delays operation for the given amount of time
# goto [url] - opens Google and goes to that URL

# win [key] - presses window key + key
# ctrl [key] - presses ctrl + key
# shift [key] - presses shift + key
# up - presses up arrow key
# down - presses down arrow key

# Commands go here...
goto slate.sheridancollege.ca
ctrl t
typeenter hi :3
";


        public LoginFile(string[] lines, string path)
        {
            Name = Path.GetFileNameWithoutExtension(path);
            Lines = new List<string>(lines);

            INIParser.TryGetValue(Lines, CategoryKey, out Category);
            INIParser.TryGetInt(Lines, OrderKey, out Order, -1);
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
                return null;
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

        public static void CreateEmpty(string directory, string name)
        {
            directory = Path.Combine(Program.RootDirectory, directory);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(Path.Combine(directory, name + "." + Extension), BlankFile);
        }
    }
}
