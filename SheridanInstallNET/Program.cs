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

        public static string MasterPassword { get; private set; }
        public static SavedInfo CurrentInfo { get; private set; }

        static void Main(string[] args)
        {
            RootDirectory = Directory.GetCurrentDirectory();

            while (!Exit)
                MainLoop();
        }

        static void MainLoop()
        {
            ClearAndWriteHeader("Sheridan Install");

            InOut.WriteLine("[1] - Login to services");
            InOut.WriteLine("[2] - View/Edit saved information");
            InOut.WriteLine("[3] - Settings");

            int selection = InOut.GetSelection(1, 3);

            if (selection == 1)
                Login();
            else if (selection == 2)
                EditInfo();
            else if (selection == 3)
                Settings();
        }

        static void ClearAndWriteHeader(string title)
        {
            InOut.Clear();
            InOut.WriteLine($"{title} - Number keys to navigate, escape to go back.");
            InOut.WriteLine(LoggedIn ? "LOGGED IN" : "NOT LOGGED IN");
            InOut.Space();
        }

        
        static void Login()
        {
            if (!VerifyLogin())
                MainLoop();

            ClearAndWriteHeader("Login to services");
        }

        static void EditInfo()
        {
            if (!VerifyLogin())
                MainLoop();

            ClearAndWriteHeader("Saved Information");
        }

        static void Settings()
        {
            ClearAndWriteHeader("Settings");
        }


        static bool VerifyLogin()
        {
            if (LoggedIn)
                return true;

            ClearAndWriteHeader("Master Password");

            // If this is the first boot, create a password
            if (!SavedInfo.Exists())
            {
                InOut.WriteLine("No saved data located. Please choose a master password:");
                MasterPassword = InOut.ReadPassword();
                while (string.IsNullOrEmpty(MasterPassword))
                {
                    InOut.WriteLine("Master password cannot be empty...");
                    MasterPassword = InOut.ReadPassword();
                    LoggedIn = true;
                }
            }
            // Otherwise, read it and make sure it is correct.
            else
            {

            }

            if (!InOut.ReadStringEscapable(out string inputPassword))
                return false;
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
