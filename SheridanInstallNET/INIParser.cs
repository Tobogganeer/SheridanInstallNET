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

        public static bool TryGetValue(List<string> lines, string key, out string value)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].StartsWith(key))
                {
                    string[] sides = lines[i].Split(SplitChar, 2, StringSplitOptions.None);
                    if (sides.Length == 2)
                    {
                        value = sides[1];
                        return true;
                    }    
                }
            }

            value = null;
            return false;
        }

        public static bool TryGetBool(List<string> lines, string key, out bool value)
        {
            if (!TryGetValue(lines, key, out string textValue))
            {
                value = false;
                return false;
            }

            return bool.TryParse(textValue, out value);
        }
    }
}
