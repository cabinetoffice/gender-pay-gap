using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace GenderPayGap.Extensions
{
    public static class Encryption
    {

        private static string DefaultEncryptionKey = "BA9138B8C0724F168A05482456802405";

        public static Encoding EncryptionEncoding = Encoding.UTF8;
        
        public static void SetDefaultEncryptionKey(string defaultEncryptionKey)
        {
            if (!string.IsNullOrWhiteSpace(defaultEncryptionKey))
            {
                DefaultEncryptionKey = defaultEncryptionKey;
            }
        }

        #region Shared Functions

        /// <summary>
        ///     Generate salt from password.
        /// </summary>
        /// <param name="password">Password string.</param>
        /// <returns>Salt bytes.</returns>
        private static byte[] SaltFromPassword(string password)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            HMACSHA1 hmac;
            hmac = new HMACSHA1(passwordBytes);
            byte[] salt = hmac.ComputeHash(passwordBytes);
            return salt;
        }


        private static ICryptoTransform GetTransform(string password, bool encrypt)
        {
            // Create an instance of the Rihndael class. 
            var cipher = new RijndaelManaged();
            // Calculate salt to make it harder to guess key by using a dictionary attack.
            byte[] salt = SaltFromPassword(password);
            // Generate Secret Key from the password and salt.
            // Note: Set number of iterations to 10 in order for JavaScript example to work faster.
            var secretKey = new Rfc2898DeriveBytes(password, salt, 10);
            // Create a encryptor from the existing SecretKey bytes by using
            // 32 bytes (256 bits) for the secret key and
            // 16 bytes (128 bits) for the initialization vector (IV).
            byte[] key = secretKey.GetBytes(32);
            byte[] iv = secretKey.GetBytes(16);
            ICryptoTransform cryptor = null;
            if (encrypt)
            {
                cryptor = cipher.CreateEncryptor(key, iv);
            }
            else
            {
                cryptor = cipher.CreateDecryptor(key, iv);
            }

            return cryptor;
        }

        /// <summary>
        ///     Encrypt/Decrypt with Write method.
        /// </summary>
        /// <param name="cryptor"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        private static byte[] CipherStreamWrite(ICryptoTransform cryptor, byte[] input)
        {
            var inputBuffer = new byte[input.Length];
            // Copy data bytes to input buffer.
            Buffer.BlockCopy(input, 0, inputBuffer, 0, inputBuffer.Length);
            byte[] outputBuffer;
            // Create a MemoryStream to hold the output bytes.
            using (var stream = new MemoryStream())
            {
                // Create a CryptoStream through which we are going to be processing our data.
                CryptoStreamMode mode;
                mode = CryptoStreamMode.Write;
                CryptoStream cryptoStream;
                using (cryptoStream = new CryptoStream(stream, cryptor, mode))
                {
                    // Start the crypting process.
                    cryptoStream.Write(inputBuffer, 0, inputBuffer.Length);
                    // Finish crypting.
                    cryptoStream.FlushFinalBlock();
                    // Convert data from a memoryStream into a byte array.
                    outputBuffer = stream.ToArray();
                    // Close both streams.
                }
            }

            return outputBuffer;
        }

        #endregion

        #region AES-256 Encryption

        private static readonly byte[] PrimerBytes = {0, 0, 0, 0};

        //Check the unencrypted data is delimited by the primer bytes
        public static bool WasEncrypted(this byte[] bytes)
        {
            return bytes.IsWrapped(PrimerBytes, PrimerBytes);
        }

        public static byte[] Encrypt(byte[] bytes, string password = null)
        {
            password = DefaultEncryptionKey + password;

            // Create a encryptor.
            ICryptoTransform encryptor = GetTransform(password, true);

            //Wrap in the primer bytes so we can later check if the decryption was correct for the key
            bytes = bytes.Wrap(PrimerBytes, PrimerBytes);

            // Return encrypted bytes.
            bytes = CipherStreamWrite(encryptor, bytes);

            return Compress(bytes);
        }

        public static string Encrypt(string text, string password = null, bool base64Encode = true)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return text;
            }

            // Turn input strings into a byte array.
            byte[] bytes = EncryptionEncoding.GetBytes(text);

            // Get encrypted bytes.
            byte[] encryptedBytes = Encrypt(bytes, password);

            if (base64Encode)
            {
                return Convert.ToBase64String(encryptedBytes);
            }

            return EncryptionEncoding.GetString(encryptedBytes);
        }

        /// <summary>
        ///     Decrypt string with AES-256 by using password key.
        /// </summary>
        /// <param name="password">String password.</param>
        /// <param name="base64reply">Encrypted Base64 string.</param>
        /// <returns>Decrypted string.</returns>
        public static string EncryptQuerystring(string querystring, string password = null, params string[] excludeNames)
        {
            if (string.IsNullOrWhiteSpace(querystring))
            {
                return querystring;
            }

            NameValueCollection nsEncrypted = querystring.FromQueryString();
            var nsDecrypted = new NameValueCollection();
            if (excludeNames != null)
            {
                foreach (string name in excludeNames)
                {
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        continue;
                    }

                    nsDecrypted[name] = nsEncrypted[name];
                    nsEncrypted.Remove(name);
                }
            }

            querystring = Encrypt(nsEncrypted.ToQueryString(), password);
            if (!string.IsNullOrEmpty(querystring))
            {
                querystring = querystring.Replace('+', '-');
                querystring = querystring.Replace('/', '_');
                querystring = querystring.Replace('=', '!');
            }

            nsDecrypted[null] = string.IsNullOrWhiteSpace(nsEncrypted[null]) ? querystring : "," + querystring;

            return nsDecrypted.ToQueryString();
        }

        public static string EncryptModel<TModel>(TModel model)
        {
            string modelSerialized = JsonConvert.SerializeObject(model);
            string encString = EncryptData(modelSerialized);
            return encString.EncodeUrlBase64();
        }

        #endregion

        #region AES-256 Decryption

        //[DebuggerStepThrough]
        public static byte[] Decrypt(byte[] bytes, params string[] passwords)
        {
            return Decrypt(bytes, new List<string>(passwords));
        }

        //[DebuggerStepThrough]
        public static byte[] Decrypt(byte[] encryptedBytes, List<string> passwords)
        {
            //Ensure the bytes are decompressed
            encryptedBytes = Decompress(encryptedBytes);

            //Always try using just the basic master password
            passwords.Add(null);

            // First, try with encryption primer 
            bool tryWithPrimer = true;
            bool found = DecryptBytes(encryptedBytes, passwords, tryWithPrimer, out byte[] decryptedBytes);

            // Try again without encryption primer 
            if (!found)
            {
                tryWithPrimer = false;
                found = DecryptBytes(encryptedBytes, passwords, tryWithPrimer, out decryptedBytes);
            }

            if (!found)
            {
                throw new CryptographicException("Could not decrypt using specified keys");
            }

            if (tryWithPrimer)
            {
                decryptedBytes = decryptedBytes.Strip(PrimerBytes.Length, PrimerBytes.Length);
            }

            return decryptedBytes;
        }

        private static bool DecryptBytes(byte[] encryptedBytes, List<string> passwords, bool tryWithPrimer, out byte[] decryptedBytes)
        {
            var found = false;
            decryptedBytes = null;
            var attempted = new HashSet<string>();
            for (var p = 0; p < passwords.Count; p++)
            {
                if (attempted.Contains(passwords[p]))
                {
                    continue;
                }

                //Skip all but the last empty password
                if (p != passwords.Count - 1 && string.IsNullOrWhiteSpace(passwords[p]))
                {
                    continue;
                }

                attempted.Add(passwords[p]);

                ICryptoTransform decryptor;
                var decrypted = false;
                try
                {
                    decryptor = GetTransform(DefaultEncryptionKey + passwords[p], false);

                    decryptedBytes = CipherStreamWrite(decryptor, encryptedBytes);

                    decryptedBytes = Decompress(decryptedBytes);

                    decrypted = true;
                }
                catch (CryptographicException) { }

                if (decrypted && (!tryWithPrimer || decryptedBytes.WasEncrypted()))
                {
                    found = true;
                    break;
                }
            }

            return found;
        }

        [DebuggerStepThrough]
        public static string Decrypt(string text, bool base64Encoded = true, params string[] passwords)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return text;
            }

            // Convert Base64 string into a byte array. 
            byte[] bytes;
            if (base64Encoded)
            {
                text = text.Replace("\n", "");
                text = text.Replace("\r", "");
                text = text.Replace(" ", "");
                text = text.Trim();

                bytes = Convert.FromBase64String(text);
            }
            else
            {
                bytes = EncryptionEncoding.GetBytes(text);
            }

            // Return decrypted string.   
            bytes = Decrypt(bytes, passwords);

            return EncryptionEncoding.GetString(bytes);
        }

        /// <summary>
        ///     Decrypt string with AES-256 by using password key.
        /// </summary>
        /// <param name="password">String password.</param>
        /// <param name="querystring">Encrypted Base64 string.</param>
        /// <returns>Decrypted string.</returns>
        //[DebuggerStepThrough]
        public static string DecryptQuerystring(string querystring, params string[] passwords)
        {
            if (string.IsNullOrWhiteSpace(querystring))
            {
                return querystring;
            }

            NameValueCollection ns = querystring.FromQueryString();
            querystring = ns[null];
            ns.Remove(null);
            ns = new NameValueCollection(ns);
            foreach (string qs in querystring.SplitI(","))
            {
                querystring = qs.Replace('-', '+');
                querystring = querystring.Replace('_', '/');
                querystring = querystring.Replace('!', '=');

                querystring = Decrypt(querystring, true, passwords);
                NameValueCollection ns2 = querystring.FromQueryString();
                foreach (string key in ns2.Keys)
                {
                    ns.Add(key, ns2[key]);
                }
            }

            return ns.ToQueryString();
        }

        public static TModel DecryptModel<TModel>(string encText)
        {
            string serializedModel = DecryptData(encText.DecodeUrlBase64());
            return JsonConvert.DeserializeObject<TModel>(serializedModel);
        }

        #endregion

        #region Compression

        private static readonly byte[] GZipHeaderBytes = {0x1f, 0x8b, 8, 0, 0, 0, 0, 0, 4, 0};
        private static readonly byte[] GZipLevel10HeaderBytes = {0x1f, 0x8b, 8, 0, 0, 0, 0, 0, 2, 0};
        private static readonly byte[] GZipLevel12HeaderBytes = {0x1f, 0x8b, 8, 0, 0, 0, 0, 0, 0, 11};

        public static bool IsCompressed(this byte[] bytes)
        {
            if (bytes.Length <= 14)
            {
                return false;
            }

            byte[] header = bytes.SubArray(4, 10);

            if (header.SequenceEqual(GZipLevel12HeaderBytes)
                || header.SequenceEqual(GZipHeaderBytes)
                || header.SequenceEqual(GZipLevel10HeaderBytes))
            {
                return true;
            }

            header = bytes.SubArray(0, 10);

            return header.SequenceEqual(GZipLevel12HeaderBytes)
                   || header.SequenceEqual(GZipHeaderBytes)
                   || header.SequenceEqual(GZipLevel10HeaderBytes);
        }

        /// <summary>
        ///     Compresses the string.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static byte[] Compress(byte[] buffer, bool mandatory = false)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
                {
                    gZipStream.Write(buffer, 0, buffer.Length);
                }

                memoryStream.Position = 0;

                var compressedData = new byte[memoryStream.Length];
                memoryStream.Read(compressedData, 0, compressedData.Length);

                var gZipBuffer = new byte[compressedData.Length + 4];
                Buffer.BlockCopy(compressedData, 0, gZipBuffer, 4, compressedData.Length);
                Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gZipBuffer, 0, 4);
                if (!mandatory && buffer.Length <= gZipBuffer.Length)
                {
                    return buffer;
                }

                return gZipBuffer;
            }
        }

        /// <summary>
        ///     Decompresses the string.
        /// </summary>
        /// <param name="compressedText">The compressed text.</param>
        /// <returns></returns>
        public static byte[] Decompress(byte[] gZipBuffer)
        {
            if (gZipBuffer == null || gZipBuffer.Length < 1 || !gZipBuffer.IsCompressed())
            {
                return gZipBuffer;
            }

            using (var memoryStream = new MemoryStream())
            {
                int dataLength = BitConverter.ToInt32(gZipBuffer, 0);
                memoryStream.Write(gZipBuffer, 4, gZipBuffer.Length - 4);

                var buffer = new byte[dataLength];

                memoryStream.Position = 0;
                using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    gZipStream.Read(buffer, 0, buffer.Length);
                }

                return buffer;
            }
        }

        #endregion

        #region Public General Encryption Methods

        public static bool IsEncryptedData(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return false;
            }

            return data.StartsWith("===") && data.EndsWith("===");
        }

        public static bool IsPrivateData(this string data, params string[] passwords)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return false;
            }

            if (IsEncryptedData(data))
            {
                data = Decrypt(data.Substring(3, data.Length - 6), true, passwords);
            }

            if (data.StartsWithI("##mkPrivatePassword:"))
            {
                return true;
            }

            if (data.StartsWithI("mkRecipientPassword:"))
            {
                return true;
            }

            return false;
        }

        public static string EncryptData(string data, bool isPrivate = false, string password = null)
        {
            if (IsEncryptedData(data))
            {
                data = Decrypt(data.Substring(3, data.Length - 6), true, password);
            }

            if (string.IsNullOrWhiteSpace(data))
            {
                return null;
            }

            if (isPrivate && !IsPrivateData(data, password))
            {
                data = "##mkPrivatePassword:" + data;
            }

            return "===" + Encrypt(data, password) + "===";
        }

        public static string DecryptData(string data)
        {
            bool isPrivate;
            return DecryptData(data, out isPrivate);
        }

        public static string DecryptData(string data, out bool isPrivate, params string[] passwords)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                isPrivate = false;
                return null;
            }

            if (IsEncryptedData(data))
            {
                data = Decrypt(data.Substring(3, data.Length - 6), true, passwords);
            }

            isPrivate = data.IsPrivateData(passwords);
            if (isPrivate)
            {
                return data.Substring(20);
            }

            return data;
        }

        #endregion

    }
}
