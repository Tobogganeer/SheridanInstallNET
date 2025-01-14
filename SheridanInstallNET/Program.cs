using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace SheridanInstallNET
{
    public class Program
    {
        public static string RootDirectory { get; private set; }

        static void Main(string[] args)
        {
            RootDirectory = Directory.GetCurrentDirectory();
            Console.WriteLine(RootDirectory);
            Console.ReadKey();
        }
    }
}
