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
        public static readonly string LoginFileFolder = "Logins";
        static bool Exit = false;
        static bool LoggedIn = false;

        public static string MasterPassword;
        public static SavedInfo CurrentInfo { get; private set; }
        public static List<LoginFile> LoginFiles { get; private set; }
        public static List<LoginCategory> LoginCategories { get; private set; }

        static void Main(string[] args)
        {
            RootDirectory = Directory.GetCurrentDirectory();
            // Make sure logins folder exists
            Directory.CreateDirectory(Path.Combine(RootDirectory, LoginFileFolder));
            LoadLoginFiles();

            while (!Exit)
                MainLoop();
        }

        static void MainLoop()
        {
            ClearAndWriteHeader("Sheridan Install");

            InOut.WriteLine("[1] - Login to services");
            InOut.WriteLine("[2] - View/Edit saved information");
            InOut.WriteLine("[3] - Settings");
            InOut.WriteLine("[4] - Exit");

            int selection = InOut.GetSelection(1, 4, true);

            if (selection == 1)
                Login();
            else if (selection == 2)
                EditInfo();
            else if (selection == 3)
                Settings();
            else if (selection == 4)
                Exit = InOut.Confirm();
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

            // No login files - ask them to make some
            if (LoginFiles.Count == 0)
            {
                InOut.WriteLine("No Login files found - import/create some in Settings.");
                InOut.WaitForInput();
                Settings();
                return;
            }

            InOut.WriteLine("[1] - Log in to selected services");
            InOut.Space();

            InOut.WriteLine("=== SERVICES ===");
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

            InOut.WriteLine("[1] - Create empty Login file");
            InOut.WriteLine("[2] - Create default Login files");
            InOut.WriteLine("[3] - Open Login file folder");
            InOut.WriteLine("[4] - Reload Login files");
            InOut.WriteLine("[5] - Delete all saved emails and passwords");
            if (LoggedIn)
                InOut.WriteLine("[6] - Log out");

            if (!InOut.GetSelectionEscapable(1, LoggedIn ? 5 : 4, out int selection))
                return;

            if (selection == 1)
            {
                DefaultLoginFiles.CreateEmpty(LoginFileFolder, "SampleLogin");
                InOut.ClearShowMessageAndWait("Created SampleLogin.login");
                Settings();
                return;
            }
            else if (selection == 2)
            {
                DefaultLoginFiles.CreateDefault(LoginFileFolder);
            }
            else if (selection == 3)
                System.Diagnostics.Process.Start("explorer.exe", Path.Combine(RootDirectory, LoginFileFolder));
            else if (selection == 4)
            {
                LoadLoginFiles();
                InOut.ClearShowMessageAndWait($"Loaded {LoginFiles?.Count} Login files.");
                Settings();
            }
            else if (selection == 5)
            {
                if (InOut.Confirm("Delete all saved emails and passwords? Cannot be undone."))
                {
                    LoggedIn = false;
                    SavedInfo.DeleteFile();
                    CurrentInfo = null;
                    MasterPassword = null;

                    InOut.ClearShowMessageAndWait("Deleted saved info.");
                    Settings();
                    return;
                }
                else
                {
                    // Loop again
                    Settings();
                }
            }
            else if (selection == 6)
            {
                if (InOut.Confirm("Log out?"))
                {
                    LoggedIn = false;
                    MasterPassword = null;
                    CurrentInfo = null;
                }
                else
                    // Loop again
                    Settings();
            }
        }

        /// <summary>
        /// Prompts the user to input the master password. Returns whether or not they are logged in.
        /// </summary>
        /// <returns></returns>
        static bool VerifyLogin()
        {
            if (LoggedIn)
                return true;

            ClearAndWriteHeader("Master Password");

            SavedInfo info = SavedInfo.Load();

            // If this is the first boot or info is corrupt, create a password
            if (info == null)
            {
                CreateBlankInfo();
                if (!LoggedIn) // User pressed escape - go back
                    return false;
            }
            // Otherwise, read it and make sure it is correct.
            else
            {
                CurrentInfo = info;
                InOut.WriteLine("Please enter the master password: ");
                if (!InOut.ReadPasswordEscapable(out MasterPassword))
                    return false;

                while (!info.IsCorrectPassword(MasterPassword))
                {
                    InOut.WriteLine("Incorrect password.");
                    if (!InOut.ReadPasswordEscapable(out MasterPassword))
                        return false;
                }

                // Correct password
                LoggedIn = true;
                CurrentInfo.FillEntriesFromData(MasterPassword);
                return true;
            }

            return LoggedIn;
        }

        static void CreateBlankInfo()
        {
            LoggedIn = false;
            InOut.WriteLine("No saved data located. Please choose a master password:");
            if (!InOut.ReadPasswordEscapable(out MasterPassword))
                return;

            while (string.IsNullOrEmpty(MasterPassword))
            {
                InOut.WriteLine("Master password cannot be empty...");
                if (!InOut.ReadPasswordEscapable(out MasterPassword))
                    return;
            }

            LoggedIn = true;
            CurrentInfo = new SavedInfo(MasterPassword);
        }

        static void LoadLoginFiles()
        {
            LoginFiles = LoginFile.LoadAll(LoginFileFolder);
            LoginCategories = LoginCategory.GetCategories(LoginFiles);
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
  - Create default Login files
  - Open Login folder
  - Reload Login files
  - Delete all saved data
  - [if logged in] Log out

*/
