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
        static bool Exit = false;
        static bool LoggedIn = false;

        static void Main(string[] args)
        {
            RootDirectory = Directory.GetCurrentDirectory();

            while (!Exit)
                MainLoop();
        }

        static void MainLoop()
        {
            ClearAndWriteHeader();

            InOut.WriteLine("[1] - Login");
            InOut.WriteLine("[2] - View/Edit saved information");
            InOut.WriteLine("[3] - Settings");

            int selection = InOut.GetSelection(1, 3);
            InOut.WriteLine(selection);
            InOut.WaitForInput();
        }

        static void ClearAndWriteHeader()
        {
            InOut.Clear();
            InOut.WriteLine("Sheridan Install - Number keys to navigate, escape to go back.");
            InOut.WriteLine(LoggedIn ? "LOGGED IN" : "NOT LOGGED IN");
            InOut.Space();
        }
    }
}

/*

Layout:
- Login
 Gate by asking for master password
  - [select categories]
  - [Next page]
  - Login to [#] services

- Edit info
 Gate by asking for master password
  - Master password
    - View master password
    - Change master password
  - [List all logins/services]
    - Show name, email *** password ***
    - On selection
      - [if info empty] Set email/set password
      - View email
      - Change email
      - View password
      - Change password

- Settings
  - Create empty Login file
  - Open Login folder
  - Delete all saved data
  - [if logged in] Log out

*/
