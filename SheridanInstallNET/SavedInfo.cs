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

        public readonly byte[] Data;
        public readonly ArraySegment<byte> MasterSalt;
        public readonly ArraySegment<byte> MasterHash;
        public readonly Dictionary<string, Entry> Entries;

        public byte NumEntriesAtTimeOfReading => Data[64 + 256];

        public SavedInfo(byte[] data)
        {
            // Data has at least enough bytes for salt, hash, and # services
            Data = data;
            MasterSalt = new ArraySegment<byte>(data, 0, 64);
            MasterHash = new ArraySegment<byte>(data, 64, 256);

            // Initialize Entries but leave it blank - we don't know password yet
            Entries = new Dictionary<string, Entry>(NumEntriesAtTimeOfReading);
        }

        public static bool Exists()
        {
            return File.Exists(Path.Combine(Program.RootDirectory, DataFile));
        }

        public static SavedInfo Load()
        {
            string filePath = Path.Combine(Program.RootDirectory, DataFile);
            byte[] data = File.ReadAllBytes(filePath);
            if (data.Length < 64 + 256 + 1)
            {
                InOut.WriteLine("DB file too small (too few bytes for master password). Erasing...");
                File.Delete(filePath);
                return null;
            }

            return new SavedInfo(data);
        }

        public class Entry
        {
            public string email;
            public string password;

            public Entry(string email, string password)
            {
                this.email = email;
                this.password = password;
            }
        }
    }
}

/*

File layout (binary)

- master password salt (64 bytes)
- master password hash (256 bytes) // used to make sure pw is correct/won't read invalid data

- num services/logins (1 byte)
- services
  - name length (1 byte)
  - name (utf 8)
  - email bytes length (4 bytes)
  - email bytes
  - password bytes length (4 bytes)
  - password bytes

*/
