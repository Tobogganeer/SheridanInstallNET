using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SheridanInstallNET
{
    public class InOut
    {
        // For reading values
        public delegate bool TryParse<T1>(string value, out T1 result);

        #region Wrappers
        public static void WriteLine(string value)
        {
            Console.WriteLine(value);
        }

        public static void WriteLine()
        {
            Console.WriteLine();
        }

        public static void WriteLine(int value)
        {
            Console.WriteLine(value);
        }

        public static void WriteLine(float value)
        {
            Console.WriteLine(value);
        }

        public static void WriteLine(bool value)
        {
            Console.WriteLine(value);
        }

        public static void Space()
        {
            Console.WriteLine();
        }

        public static void Write(string value)
        {
            Console.Write(value);
        }

        public static string ReadLine()
        {
            return Console.ReadLine();
        }

        public static ConsoleKeyInfo ReadKey(bool intercept = false)
        {
            return Console.ReadKey(intercept);
        }

        public static void Clear()
        {
            Console.Clear();
        }
        #endregion

        public static void WaitForInput(string prompt = null)
        {
            WriteLine(prompt ?? "Press any key to continue...");
            ReadKey();
        }

        public static string ReadString(string prompt)
        {
            Write(prompt ?? "Input a string: ");
            return ReadLine();
        }

        public static int GetSelection(int min, int max)
        {
            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (int.TryParse(key.KeyChar.ToString(), out int res) && res >= min && res <= max)
                    return res;
            }
        }

        public static bool GetSelectionEscapable(int min, int max, out int selection)
        {
            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (int.TryParse(key.KeyChar.ToString(), out int res) && res >= min && res <= max)
                {
                    selection = res;
                    return true;
                }
                else if (key.Key == ConsoleKey.Escape)
                {
                    selection = 0;
                    return false;
                }
            }
        }

        // Return true on successful input, false if user presses escape
        public static bool ReadStringEscapable(out string output)
        {
            output = "";
            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Enter)
                {
                    Write("\n");
                    return true;
                }
                else if (key.Key == ConsoleKey.Escape)
                {
                    Write("\n");
                    return false;
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    if (output.Length > 0)
                    {
                        output = output.Remove(output.Length - 1);
                        Console.CursorLeft--;
                        Write(" ");
                        Console.CursorLeft--;
                    }
                    // Back up, write a blank, then back up again
                }
                else
                {
                    // Basically write every char to the output, and check for escapes
                    // Like a sillier way of Console.ReadLine()
                    output += key.KeyChar;
                    Console.Write(key.KeyChar); // InOut doesn't do chars so yknow
                }
            }
        }

        public static string ReadPassword()
        {
            // https://stackoverflow.com/questions/3404421/password-masking-console-application
            string pass = string.Empty;
            ConsoleKey key;
            do
            {
                var keyInfo = Console.ReadKey(intercept: true);
                key = keyInfo.Key;

                if (key == ConsoleKey.Backspace && pass.Length > 0)
                {
                    Console.Write("\b \b");
                    pass = pass.Substring(0, pass.Length - 1);
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    Console.Write("*");
                    pass += keyInfo.KeyChar;
                }
            } while (key != ConsoleKey.Enter);

            Console.WriteLine();
            return pass;
        }

        public static bool ReadPasswordEscapable(out string password)
        {
            // https://stackoverflow.com/questions/3404421/password-masking-console-application
            string pass = string.Empty;
            ConsoleKey key;
            do
            {
                var keyInfo = Console.ReadKey(intercept: true);
                key = keyInfo.Key;

                if (key == ConsoleKey.Backspace && pass.Length > 0)
                {
                    Console.Write("\b \b");
                    pass = pass.Substring(0, pass.Length - 1);
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    Console.Write("*");
                    pass += keyInfo.KeyChar;
                }
                else if (key == ConsoleKey.Escape)
                {
                    password = null;
                    return false;
                }
            } while (key != ConsoleKey.Enter);

            Console.WriteLine();
            password = pass;
            return true;
        }

        public static bool ReadStringEscapablePredicate(out string output,
            Predicate<string> condition, string optionsText)
        {
            while (ReadStringEscapable(out string firstOutput))
            {
                if (condition(firstOutput))
                {
                    output = firstOutput;
                    return true;
                }
                else
                {
                    Write($"Valid options: {optionsText}: ");
                }
            }

            output = null;
            return false;
        }

        // Generic read method
        public static T ReadValue<T>(string prompt, TryParse<T> tryParse)
        {
            string read = ReadString(prompt ?? $"Input a {typeof(T).Name}: ");

            while (true)
            {
                if (tryParse(read, out T value))
                {
                    return value;
                }
                else
                {
                    read = ReadString($"Input a valid {typeof(T).Name}: ");
                }
            }
        }

        // Shortcuts for various simple types (forget the name I am looking for)
        public static int ReadInt(string prompt) => ReadValue<int>(prompt, int.TryParse);
        public static long ReadLong(string prompt) => ReadValue<long>(prompt, long.TryParse);
        public static float ReadFloat(string prompt) => ReadValue<float>(prompt, float.TryParse);
        public static double ReadDouble(string prompt) => ReadValue<double>(prompt, double.TryParse);

        public static string ReadStringPredicate(string prompt, Predicate<string> condition, string validOptions)
        {
            if (prompt == null || condition == null || validOptions == null)
            {
                // Maybe allow prompt to be null? Or just return ReadString()?
                throw new ArgumentNullException("Must supply arguments for ReadStringPredicate!");
            }

            string read = ReadString(prompt);

            while (true)
            {
                if (condition(read))
                {
                    return read;
                }
                else
                {
                    read = ReadString($"Valid options: {validOptions}: ");
                }
            }
        }

        public static bool Confirm(string prompt = null)
        {
            Write((prompt ?? "Are you sure?") + " [y/n]: ");

            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Y) return true;
                if (key.Key == ConsoleKey.N) return false;
            }
        }
    }
}
