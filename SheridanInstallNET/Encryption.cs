﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SheridanInstallNET
{
    public static class Encryption
    {
        private static readonly RandomNumberGenerator Random = RandomNumberGenerator.Create();

        //Preconfigured Encryption Parameters
        public static readonly int BlockBitSize = 128;
        public static readonly int KeyBitSize = 256;

        //Preconfigured Password Key Derivation Parameters
        public static readonly int SaltBitSize = 64;
        public static readonly int Iterations = 10000;
        public static readonly int MinPasswordLength = 4;

        public static byte[] CreateSalt(int numBytes)
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] buff = new byte[numBytes];
            rng.GetBytes(buff);
            return buff;
        }

        public static byte[] ComputeSha256Hash(string rawData, byte[] salt)
        {
            // Create a SHA256
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] textBytes = Encoding.UTF8.GetBytes(rawData);
                Array.Resize(ref textBytes, textBytes.Length + salt.Length);
                Array.Copy(salt, 0, textBytes, textBytes.Length - salt.Length, salt.Length);
                byte[] bytes = sha256Hash.ComputeHash(textBytes);

                return bytes;
            }
        }

        public static string SimpleEncryptWithPassword(string secretMessage, string password,
                                 byte[] nonSecretPayload = null)
        {
            if (string.IsNullOrEmpty(secretMessage))
                throw new ArgumentException("Secret Message Required!", "secretMessage");

            var plainText = Encoding.UTF8.GetBytes(secretMessage);
            var cipherText = SimpleEncryptWithPassword(plainText, password, nonSecretPayload);
            return Convert.ToBase64String(cipherText);
        }

        public static string SimpleDecryptWithPassword(string encryptedMessage, string password,
                                 int nonSecretPayloadLength = 0)
        {
            if (string.IsNullOrWhiteSpace(encryptedMessage))
                throw new ArgumentException("Encrypted Message Required!", "encryptedMessage");

            var cipherText = Convert.FromBase64String(encryptedMessage);
            var plainText = SimpleDecryptWithPassword(cipherText, password, nonSecretPayloadLength);
            return plainText == null ? null : Encoding.UTF8.GetString(plainText);
        }

        public static byte[] SimpleEncrypt(byte[] secretMessage, byte[] cryptKey, byte[] authKey, byte[] nonSecretPayload = null)
        {
            //User Error Checks
            if (cryptKey == null || cryptKey.Length != KeyBitSize / 8)
                throw new ArgumentException(String.Format("Key needs to be {0} bit!", KeyBitSize), "cryptKey");

            if (authKey == null || authKey.Length != KeyBitSize / 8)
                throw new ArgumentException(String.Format("Key needs to be {0} bit!", KeyBitSize), "authKey");

            if (secretMessage == null || secretMessage.Length < 1)
                throw new ArgumentException("Secret Message Required!", "secretMessage");

            //non-secret payload optional
            nonSecretPayload = nonSecretPayload ?? new byte[] { };

            byte[] cipherText;
            byte[] iv;

            using (var aes = Aes.Create())
            {
                /*
                KeySize = KeyBitSize,
                BlockSize = BlockBitSize,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
                */

                aes.KeySize = KeyBitSize;
                aes.BlockSize = BlockBitSize;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                //Use random IV
                aes.GenerateIV();
                iv = aes.IV;

                using (var encrypter = aes.CreateEncryptor(cryptKey, iv))
                using (var cipherStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(cipherStream, encrypter, CryptoStreamMode.Write))
                    using (var binaryWriter = new BinaryWriter(cryptoStream))
                    {
                        //Encrypt Data
                        binaryWriter.Write(secretMessage);
                    }

                    cipherText = cipherStream.ToArray();
                }

            }

            //Assemble encrypted message and add authentication
            using (var hmac = new HMACSHA256(authKey))
            using (var encryptedStream = new MemoryStream())
            {
                using (var binaryWriter = new BinaryWriter(encryptedStream))
                {
                    //Prepend non-secret payload if any
                    binaryWriter.Write(nonSecretPayload);
                    //Prepend IV
                    binaryWriter.Write(iv);
                    //Write Ciphertext
                    binaryWriter.Write(cipherText);
                    binaryWriter.Flush();

                    //Authenticate all data
                    var tag = hmac.ComputeHash(encryptedStream.ToArray());
                    //Postpend tag
                    binaryWriter.Write(tag);
                }
                return encryptedStream.ToArray();
            }

        }

        public static byte[] SimpleDecrypt(byte[] encryptedMessage, byte[] cryptKey, byte[] authKey, int nonSecretPayloadLength = 0)
        {

            //Basic Usage Error Checks
            if (cryptKey == null || cryptKey.Length != KeyBitSize / 8)
                throw new ArgumentException(String.Format("CryptKey needs to be {0} bit!", KeyBitSize), "cryptKey");

            if (authKey == null || authKey.Length != KeyBitSize / 8)
                throw new ArgumentException(String.Format("AuthKey needs to be {0} bit!", KeyBitSize), "authKey");

            if (encryptedMessage == null || encryptedMessage.Length == 0)
                throw new ArgumentException("Encrypted Message Required!", "encryptedMessage");

            using (var hmac = new HMACSHA256(authKey))
            {
                var sentTag = new byte[hmac.HashSize / 8];
                //Calculate Tag
                var calcTag = hmac.ComputeHash(encryptedMessage, 0, encryptedMessage.Length - sentTag.Length);
                var ivLength = (BlockBitSize / 8);

                //if message length is to small just return null
                if (encryptedMessage.Length < sentTag.Length + nonSecretPayloadLength + ivLength)
                    return null;

                //Grab Sent Tag
                Array.Copy(encryptedMessage, encryptedMessage.Length - sentTag.Length, sentTag, 0, sentTag.Length);

                //Compare Tag with constant time comparison
                var compare = 0;
                for (var i = 0; i < sentTag.Length; i++)
                    compare |= sentTag[i] ^ calcTag[i];

                //if message doesn't authenticate return null
                if (compare != 0)
                    return null;

                using (var aes = Aes.Create())
                {
                    aes.KeySize = KeyBitSize;
                    aes.BlockSize = BlockBitSize;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    //Grab IV from message
                    var iv = new byte[ivLength];
                    Array.Copy(encryptedMessage, nonSecretPayloadLength, iv, 0, iv.Length);

                    using (var decrypter = aes.CreateDecryptor(cryptKey, iv))
                    using (var plainTextStream = new MemoryStream())
                    {
                        using (var decrypterStream = new CryptoStream(plainTextStream, decrypter, CryptoStreamMode.Write))
                        using (var binaryWriter = new BinaryWriter(decrypterStream))
                        {
                            //Decrypt Cipher Text from Message
                            binaryWriter.Write(
                              encryptedMessage,
                              nonSecretPayloadLength + iv.Length,
                              encryptedMessage.Length - nonSecretPayloadLength - iv.Length - sentTag.Length
                            );
                        }
                        //Return Plain Text
                        return plainTextStream.ToArray();
                    }
                }
            }
        }

        public static byte[] SimpleEncryptWithPassword(byte[] secretMessage, string password, byte[] nonSecretPayload = null)
        {
            nonSecretPayload = nonSecretPayload ?? new byte[] { };

            //User Error Checks
            if (string.IsNullOrWhiteSpace(password) || password.Length < MinPasswordLength)
                throw new ArgumentException(String.Format("Must have a password of at least {0} characters!", MinPasswordLength), "password");

            if (secretMessage == null || secretMessage.Length == 0)
                throw new ArgumentException("Secret Message Required!", "secretMessage");

            var payload = new byte[((SaltBitSize / 8) * 2) + nonSecretPayload.Length];

            Array.Copy(nonSecretPayload, payload, nonSecretPayload.Length);
            int payloadIndex = nonSecretPayload.Length;

            byte[] cryptKey;
            byte[] authKey;
            //Use Random Salt to prevent pre-generated weak password attacks.
            using (var generator = new Rfc2898DeriveBytes(password, SaltBitSize / 8, Iterations))
            {
                var salt = generator.Salt;

                //Generate Keys
                cryptKey = generator.GetBytes(KeyBitSize / 8);

                //Create Non Secret Payload
                Array.Copy(salt, 0, payload, payloadIndex, salt.Length);
                payloadIndex += salt.Length;
            }

            //Deriving separate key, might be less efficient than using HKDF, 
            //but now compatible with RNEncryptor which had a very similar wireformat and requires less code than HKDF.
            using (var generator = new Rfc2898DeriveBytes(password, SaltBitSize / 8, Iterations))
            {
                var salt = generator.Salt;

                //Generate Keys
                authKey = generator.GetBytes(KeyBitSize / 8);

                //Create Rest of Non Secret Payload
                Array.Copy(salt, 0, payload, payloadIndex, salt.Length);
            }

            return SimpleEncrypt(secretMessage, cryptKey, authKey, payload);
        }

        public static byte[] SimpleDecryptWithPassword(byte[] encryptedMessage, string password, int nonSecretPayloadLength = 0)
        {
            //User Error Checks
            if (string.IsNullOrWhiteSpace(password) || password.Length < MinPasswordLength)
                throw new ArgumentException(String.Format("Must have a password of at least {0} characters!", MinPasswordLength), "password");

            if (encryptedMessage == null || encryptedMessage.Length == 0)
                throw new ArgumentException("Encrypted Message Required!", "encryptedMessage");

            var cryptSalt = new byte[SaltBitSize / 8];
            var authSalt = new byte[SaltBitSize / 8];

            //Grab Salt from Non-Secret Payload
            Array.Copy(encryptedMessage, nonSecretPayloadLength, cryptSalt, 0, cryptSalt.Length);
            Array.Copy(encryptedMessage, nonSecretPayloadLength + cryptSalt.Length, authSalt, 0, authSalt.Length);

            byte[] cryptKey;
            byte[] authKey;

            //Generate crypt key
            using (var generator = new Rfc2898DeriveBytes(password, cryptSalt, Iterations))
            {
                cryptKey = generator.GetBytes(KeyBitSize / 8);
            }
            //Generate auth key
            using (var generator = new Rfc2898DeriveBytes(password, authSalt, Iterations))
            {
                authKey = generator.GetBytes(KeyBitSize / 8);
            }

            return SimpleDecrypt(encryptedMessage, cryptKey, authKey, cryptSalt.Length + authSalt.Length + nonSecretPayloadLength);
        }
    }
}
