using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Diagnostics;

namespace SheridanInstallNET
{
    public class Program
    {
        const int VERSION = 1;

        public static string RootDirectory { get; private set; }
        public static readonly string LoginFileFolder = "Logins";
        public static readonly string CollectionsFileFolder = "Collections";
        static bool Exit = false;
        static bool LoggedIn = false;

        public static string MasterPassword;
        public static SavedInfo CurrentInfo { get; private set; }
        public static List<LoginFile> LoginFiles { get; private set; }
        public static List<CollectionFile> CollectionFiles { get; private set; }
        //public static List<LoginCategory> LoginCategories { get; private set; }

        static void Main(string[] args)
        {
            RootDirectory = Directory.GetCurrentDirectory();
            // Make sure logins folder exists
            Directory.CreateDirectory(Path.Combine(RootDirectory, LoginFileFolder));
            Directory.CreateDirectory(Path.Combine(RootDirectory, CollectionsFileFolder));
            LoadLoginFiles();

            while (!Exit)
                MainLoop();
        }

        static void MainLoop()
        {
            ClearAndWriteHeader($"Sheridan Install (v{VERSION})");

            InOut.WriteLine("[1] - Login to services");
            InOut.WriteLine("[2] - View/Edit saved information");
            InOut.WriteLine("[3] - Settings");
            InOut.WriteLine("[4] - Exit");

            int selection = InOut.GetSelection(1, 4, true);

            if (selection == 1)
            {
                // Reset 'enabled states' when we get to this menu
                foreach (LoginFile login in LoginFiles)
                    login.enabled = login.EnabledByDefault;
                Login();
            }
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
            InOut.Write(title);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            InOut.WriteLine("  - Number keys to navigate, escape to go back.");
            Console.ForegroundColor = LoggedIn ? ConsoleColor.DarkGreen : ConsoleColor.DarkRed;
            InOut.WriteLine(LoggedIn ? "LOGGED IN" : "NOT LOGGED IN");
            Console.ResetColor();
            InOut.Space();
        }

        
        static void Login()
        {
            if (!VerifyLogin())
                MainLoop();

            ClearAndWriteHeader("HOME > Login to services");

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

            DisplayLogins();
        }

        static void DisplayLogins()
        {
            int currentNumber = 2; // Logging in is 1

            List<Action> callbacks = new List<Action>
            {
                LoginToServices
            };

            // TODO: Better order display
            LoginCategory uncategorized = LoginCategories.Find((cat) => cat.name == string.Empty);
            // Check if we have uncategorized Login files
            if (uncategorized != null)
            {
                foreach (LoginFile login in uncategorized.files)
                {
                    DisplayEnabledText($"[{currentNumber++}] - {login.Name}", login.enabled);
                    // Callback to toggle it
                    callbacks.Add(() => {
                        login.enabled = !login.enabled;
                        Login(); // Display again
                    });
                }
            }

            foreach (LoginCategory category in LoginCategories)
            {
                // We already did the uncategorized ones
                if (category.name == string.Empty)
                    continue;

                // Grey out category if all services are disabled
                DisplayEnabledText($"[{currentNumber++}] - {category.name}", !category.AllDisabled);
                foreach (LoginFile login in category.files)
                    DisplayEnabledText($"    --- {login.Name}", login.enabled);

                // Pressing on this service will go into it
                callbacks.Add(() => EditCategoryServices(category));
            }

            // Go back to main menu
            if (!InOut.GetSelectionEscapable(1, currentNumber - 1, out int selection, true))
                return;

            // Call the appropriate callback (selection starts at 1, array starts at 0)
            callbacks[selection - 1]();
        }

        static void LoginToServices()
        {
            throw new NotImplementedException();
        }

        static void DisplayEnabledText(string text, bool enabled)
        {
            if (!enabled)
            {
                // Make it gray if disabled
                //Console.BackgroundColor = ConsoleColor.Gray;
                Console.ForegroundColor = ConsoleColor.DarkGray;
                text += " (will not be logged in to)";
            }

            InOut.WriteLine(text);
            Console.ResetColor();
        }

        static void EditCategoryServices(LoginCategory category)
        {
            ClearAndWriteHeader("LOGIN > Edit enabled services from " + category.name);

            InOut.WriteLine($"=== {category.name} ===");
            InOut.WriteLine("[1] - Toggle all");
            InOut.Space();

            for (int i = 0; i < category.files.Count; i++)
                DisplayEnabledText($"[{i + 2}] - {category.files[i].Name}", category.files[i].enabled);

            if (!InOut.GetSelectionEscapable(1, category.files.Count + 1, out int selection, false))
            {
                Login(); // Go back to login page if we click escape
                return;
            }

            if (selection == 1)
            {
                bool allDisabled = category.AllDisabled;
                foreach (LoginFile login in category.files)
                    login.enabled = allDisabled;
            }
            else
            {
                // Turn this service on/off
                category.files[selection - 2].enabled = !category.files[selection - 2].enabled;
            }
            
            // Stay on this menu
            EditCategoryServices(category);
        }


        static void EditInfo()
        {
            if (!VerifyLogin())
                MainLoop();

            ClearAndWriteHeader("INFO > Saved Information");

            InOut.WriteLine("[1] - Master Password");
            InOut.Space();

            // No login files/entries
            if (CurrentInfo.CurrentEntryCount == 0)
            {
                // Give option to go to settings to create login files
                InOut.WriteLine("No entries. Please add a Login file to add data.");
                InOut.WriteLine("[2] - Settings");
                if (!InOut.GetSelectionEscapable(1, 2, out int lilSelection))
                    return;
                if (lilSelection == 1)
                    EditMasterPassword();
                else
                    Settings();
            }

            int currentNumber = 2; // Editing master password is 1

            List<Action> callbacks = new List<Action>
            {
                EditMasterPassword
            };

            foreach (KeyValuePair<string, SavedInfo.Entry> pair in CurrentInfo.Entries)
            {
                InOut.WriteLine($"[{currentNumber++}] - {pair.Key}");
                if (!string.IsNullOrEmpty(pair.Value.email))    
                    InOut.WriteLine($"  - Email: {new string('*', pair.Value.email.Length)}");
                else
                    InOut.WriteLine($"  - No Email assigned");

                if (!string.IsNullOrEmpty(pair.Value.password))
                    InOut.WriteLine($"  - Password: {new string('*', pair.Value.password.Length)}");
                else
                    InOut.WriteLine($"  - No Password assigned");
            }

            if (!InOut.GetSelectionEscapable(1, currentNumber - 1, out int selection))
                return;

            callbacks[selection - 1]();

            /*
            
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

            */
        }

        static void EditMasterPassword()
        {

        }



        static void Settings()
        {
            ClearAndWriteHeader("SETTINGS");

            InOut.WriteLine("[1] - Create empty Login file");
            InOut.WriteLine("[2] - Create default Login files");
            InOut.WriteLine("[3] - Open Login file folder");
            InOut.WriteLine("[4] - Reload Login files");
            InOut.WriteLine("[5] - Delete all saved emails and passwords");
            if (LoggedIn)
                InOut.WriteLine("[6] - Log out");

            if (!InOut.GetSelectionEscapable(1, LoggedIn ? 6 : 5, out int selection))
                return;

            if (selection == 1)
            {
                DefaultLoginFiles.CreateEmpty(LoginFileFolder, "SampleLogin");
                InOut.ClearShowMessageAndWait("Created SampleLogin.login");
                Settings();
            }
            else if (selection == 2)
            {
                DefaultLoginFiles.CreateDefault(LoginFileFolder);
                DefaultCollectionFiles.CreateDefault(CollectionsFileFolder);
                LoadLoginFiles();
                InOut.ClearShowMessageAndWait("Created default login files");
                Settings();
            }
            else if (selection == 3)
            {
                Process.Start("explorer.exe", Path.Combine(RootDirectory, LoginFileFolder));
                Settings();
            }
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
                EnsureLoginsArePresentInSavedInfo();
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
            LoadLoginFiles();
        }

        static void LoadLoginFiles()
        {
            LoginFiles = LoginFile.LoadAll(LoginFileFolder);
            CollectionFiles = CollectionFile.LoadAll(CollectionsFileFolder);
            //LoginCategories = LoginCategory.GetCategories(LoginFiles);
            EnsureLoginsArePresentInSavedInfo();
        }

        static void EnsureLoginsArePresentInSavedInfo()
        {
            if (CurrentInfo == null || LoginFiles == null)
                return;

            foreach (LoginFile login in LoginFiles)
            {
                if (!CurrentInfo.Entries.ContainsKey(login.Name))
                    CurrentInfo.Entries.Add(login.Name, new SavedInfo.Entry(login.Name, string.Empty, string.Empty));
            }
        }
    }
}

/*

Layout:
- Login
 Gate by asking for master password
  - Select specific services
    - [select services]
    - [Next page]
    - Login to [#] services
  - Select collections
    - [select collections]
    - [Next page]
  - Login to all services
  - Login to selected services (list services below)

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
