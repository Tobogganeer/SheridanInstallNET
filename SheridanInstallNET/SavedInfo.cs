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
        private const int SaltSize = 64;
        private const int PassHashSize = 256;

        public readonly byte[] Data;
        public readonly ArraySegment<byte> MasterSalt;
        public readonly ArraySegment<byte> MasterHash;

        public readonly Dictionary<string, Entry> Entries;
        public int CurrentEntryCount => Entries.Count;


        /// <summary>
        /// Creates a SavedInfo instance from the given <paramref name="data"/>
        /// </summary>
        /// <param name="data"></param>
        public SavedInfo(byte[] data)
        {
            if (data.Length < SaltSize + PassHashSize + 1)
                throw new ArgumentException("SavedInfo data too small", "data");

            // Data has at least enough bytes for salt, hash, and # services
            Data = data;
            MasterSalt = new ArraySegment<byte>(Data, 0, SaltSize);
            MasterHash = new ArraySegment<byte>(Data, SaltSize, PassHashSize);

            // Initialize Entries but leave it blank - we don't know password yet
            Entries = new Dictionary<string, Entry>(Data[SaltSize + PassHashSize]);
        }

        /// <summary>
        /// Creates an empty SavedInfo instance with the given <paramref name="masterPassword"/>
        /// </summary>
        /// <param name="masterPassword"></param>
        public SavedInfo(string masterPassword)
        {
            byte[] salt = Encryption.CreateSalt(SaltSize);
            byte[] masterPasswordHash = Encryption.ComputeSha256Hash(masterPassword, salt);
            byte[] combined = new byte[SaltSize + PassHashSize + 1];
            Array.Copy(salt, combined, SaltSize);
            Array.Copy(masterPasswordHash, 0, combined, SaltSize, PassHashSize);
            combined[SaltSize + PassHashSize] = 0; // 0 services/entries

            Data = combined;
            MasterSalt = new ArraySegment<byte>(Data, 0, SaltSize);
            MasterHash = new ArraySegment<byte>(Data, SaltSize, PassHashSize);

            // Initialize Entries but leave it blank - we don't know password yet
            Entries = new Dictionary<string, Entry>(Data[SaltSize + PassHashSize]);
        }


        public bool CorrectPassword(string candidate)
        {
            byte[] candidateHash = Encryption.ComputeSha256Hash(candidate, MasterSalt.ToArray());

            if (candidateHash.Length != MasterHash.Count)
                return false; // Shouldn't be possible? Just making sure

            // Make sure all bytes match
            for (int i = 0; i < candidateHash.Length; i++)
            {
                if (candidateHash[i] != MasterHash.Array[MasterHash.Offset + i])
                    return false;
            }

            return true;
        }

        public void FillEntriesFromData(string masterPassword)
        {
            if (!CorrectPassword(masterPassword))
                return;

            byte numServicesInData = Data[SaltSize + PassHashSize];
            int currentOffset = 0;
            for (int i = 0; i < numServicesInData; i++)
            {

            }
        }


        public static bool Exists()
        {
            return File.Exists(Path.Combine(Program.RootDirectory, DataFile));
        }

        public static SavedInfo Load()
        {
            string filePath = Path.Combine(Program.RootDirectory, DataFile);
            byte[] data = File.ReadAllBytes(filePath);
            if (data.Length < SaltSize + PassHashSize + 1)
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
