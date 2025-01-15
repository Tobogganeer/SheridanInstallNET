using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SheridanInstallNET
{
    public class INIParser
    {
        static readonly char[] SplitChar = { '=' };

        public static bool TryGetValue(List<string> lines, string key, out string value, string defaultValue = null)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].StartsWith(key))
                {
                    string[] sides = lines[i].Split(SplitChar, 2, StringSplitOptions.None);
                    if (sides.Length == 2)
                    {
                        value = sides[1].Trim();
                        return true;
                    }    
                }
            }

            value = defaultValue;
            return false;
        }

        public static bool TryGetBool(List<string> lines, string key, out bool value, bool defaultValue = false)
            => TryGet(lines, key, out value, defaultValue, bool.TryParse);

        public static bool TryGetInt(List<string> lines, string key, out int value, int defaultValue = 0)
            => TryGet(lines, key, out value, defaultValue, int.TryParse);

        public static bool TryGet<T>(List<string> lines, string key, out T value, T defaultValue, InOut.TryParse<T> tryParse)
        {
            if (!TryGetValue(lines, key, out string textValue))
            {
                value = defaultValue;
                return false;
            }

            return tryParse(textValue, out value);
        }
    }
}
