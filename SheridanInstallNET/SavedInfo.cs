using System;
using System.Collections;
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
        private const int PassHashSize = 32; // 256 bits

        public byte[] Data { get; private set; }
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

        public void ChangeMasterPassword(string newMasterPassword)
        {
            byte[] salt = Encryption.CreateSalt(SaltSize);
            byte[] masterPasswordHash = Encryption.ComputeSha256Hash(newMasterPassword, salt);
            Array.Copy(salt, Data, SaltSize);
            Array.Copy(masterPasswordHash, 0, Data, SaltSize, PassHashSize);
        }

        /// <summary>
        /// Gets the current entries, ready to be written to disk. Changes the <paramref name="masterPassword"/> if necessary.
        /// </summary>
        /// <param name="masterPassword"></param>
        /// <returns></returns>
        public byte[] GetBinaryData(string masterPassword)
        {
            if (!IsCorrectPassword(masterPassword))
                ChangeMasterPassword(masterPassword);

            List<byte[]> binaryNames = new List<byte[]>(Entries.Count);
            List<byte[]> emailBytes = new List<byte[]>(Entries.Count);
            List<byte[]> passwordBytes = new List<byte[]>(Entries.Count);

            foreach (Entry entry in Entries.Values)
            {
                if (string.IsNullOrEmpty(entry.name))
                    continue;

                binaryNames.Add(entry.GetNameBytes());
                emailBytes.Add(entry.GetEmailBytes(masterPassword));
                passwordBytes.Add(entry.GetPasswordBytes(masterPassword));
            }

            List<byte> outputData = new List<byte>();
            // Add master password and salt
            outputData.AddRange(new ArraySegment<byte>(Data, 0, SaltSize + PassHashSize));
            outputData.Add((byte)binaryNames.Count);
            for (int i = 0; i < binaryNames.Count; i++)
            {
                outputData.Add((byte)binaryNames[i].Length);
                outputData.AddRange(binaryNames[i]);

                outputData.AddRange(BitConverter.GetBytes(emailBytes[i].Length));
                outputData.AddRange(emailBytes[i]);

                outputData.AddRange(BitConverter.GetBytes(passwordBytes[i].Length));
                outputData.AddRange(passwordBytes[i]);
            }

            return outputData.ToArray();
        }


        public bool IsCorrectPassword(string candidate)
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
            if (!IsCorrectPassword(masterPassword))
                return;

            byte numServicesInData = Data[SaltSize + PassHashSize];
            int readPosition = SaltSize + PassHashSize + 1;
            for (int i = 0; i < numServicesInData; i++)
            {
                byte nameLength = Data[readPosition++];
                string name = Encoding.UTF8.GetString(Data, readPosition, nameLength);
                readPosition += nameLength;

                int emailByteCount = BitConverter.ToInt32(Data, readPosition);
                readPosition += 4;

                byte[] emailBytes = new byte[emailByteCount];
                for (int j = 0; j < emailByteCount; j++, readPosition++)
                    emailBytes[j] = Data[readPosition];

                int passwordByteCount = BitConverter.ToInt32(Data, readPosition);
                readPosition += 4;

                byte[] passwordBytes = new byte[passwordByteCount];
                for (int j = 0; j < passwordByteCount; j++, readPosition++)
                    passwordBytes[j] = Data[readPosition];

                Entries.Add(name, Entry.CreateFromBytes(masterPassword, name, emailBytes, passwordBytes));
            }
        }


        public static bool Exists()
        {
            return File.Exists(Path.Combine(Program.RootDirectory, DataFile));
        }

        public static SavedInfo Load()
        {
            if (!Exists())
                return null;

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

        /// <summary>
        /// Writes the entries to disk using the given password
        /// </summary>
        /// <param name="masterPassword"></param>
        public void Save(string masterPassword)
        {
            string filePath = Path.Combine(Program.RootDirectory, DataFile);
            byte[] data = GetBinaryData(masterPassword);
            File.WriteAllBytes(filePath, data);
        }

        /// <summary>
        /// Deletes the saved data from the disk. Cannot be undone.
        /// </summary>
        public static void DeleteFile()
        {
            if (!Exists())
                return;

            File.Delete(Path.Combine(Program.RootDirectory, DataFile));
        }

        public class Entry
        {
            public string name;
            public string email;
            public string password;

            public Entry(string name, string email, string password)
            {
                this.email = email;
                this.password = password;
            }

            public static Entry CreateFromBytes(string masterPassword, string name, byte[] emailBytes, byte[] passwordBytes)
            {
                try
                {
                    string email = Encoding.UTF8.GetString(Encryption.SimpleDecryptWithPassword(emailBytes, masterPassword));
                    string password = Encoding.UTF8.GetString(Encryption.SimpleDecryptWithPassword(passwordBytes, masterPassword));
                    return new Entry(name, email, password);
                }
                catch
                {
                    return new Entry(name, null, null);
                }
            }

            public byte[] GetNameBytes() => Encoding.UTF8.GetBytes(name);

            public byte[] GetEmailBytes(string masterPassword)
                => Encryption.SimpleEncryptWithPassword(Encoding.UTF8.GetBytes(email), masterPassword);

            public byte[] GetPasswordBytes(string masterPassword)
                => Encryption.SimpleEncryptWithPassword(Encoding.UTF8.GetBytes(password), masterPassword);
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
