using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SheridanInstallNET
{
    public class SavedInfo
    {
        private static readonly string DataFile = "db";

        //public static string 

        public static void Load()
        {
            if (!File.Exists(Path.Combine(Program.RootDirectory, DataFile)))
            {

            }
        }
    }
}
