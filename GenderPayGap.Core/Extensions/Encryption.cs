using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using GenderPayGap.Core;
using Newtonsoft.Json;

namespace GenderPayGap.Extensions
{
    public static class Encryption
    {
        private static string _defaultEncryptionKey = null;
        private static byte[] _keyBytes = null;
        private static byte[] _ivBytes = null;

        private static Encoding EncryptionEncoding = Encoding.UTF8;

        static Encryption()
        {
            if (Global.DefaultEncryptionKey != null && Global.DefaultEncryptionIv != null)
            {
                _defaultEncryptionKey = Global.DefaultEncryptionKey;
                _keyBytes = HexDecode(Global.DefaultEncryptionKey);
                _ivBytes = HexDecode(Global.DefaultEncryptionIv);
            }
        }
        
        public static void SetDefaultEncryptionKey(string defaultEncryptionKey, string defaultIv)
        {
            _defaultEncryptionKey = defaultEncryptionKey;
            _keyBytes = HexDecode(defaultEncryptionKey);
            _ivBytes = HexDecode(defaultIv);
        }

        
        # region Encrypt

        public static string EncryptModel<TModel>(TModel plainTextModel)
        {
            string plainText = JsonConvert.SerializeObject(plainTextModel);
            return EncryptString(plainText);
        }
        
        public static string EncryptId(long plainTextId)
        {
            string plainText = plainTextId.ToString();
            return EncryptString(plainText);
        }

        public static string EncryptString(string plainText)
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            
            // Encrypt
            byte[] cypherTextBytes = EncryptBytes(plainTextBytes);
            
            // Base 16 encode (so it's suitable for all uses including in a QueryString)
            string hexEncodedCypherText = HexEncode(cypherTextBytes);
            
            return hexEncodedCypherText;
        }

        private static byte[] EncryptBytes(byte[] plainTextBytes)
        {
            if (_keyBytes == null || _ivBytes == null)
            {
                throw new Exception("_keyBytes or _ivBytes not set");
            }

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = _keyBytes;
                aesAlg.IV = _ivBytes;
                
                using (ICryptoTransform encryptor = aesAlg.CreateEncryptor())
                using (MemoryStream msEncrypt = new MemoryStream())
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    csEncrypt.Write(plainTextBytes);
                    csEncrypt.FlushFinalBlock();
                    return msEncrypt.ToArray();
                }
            }
        }

        private static string HexEncode(byte[] bytesToHexEncode)
        {
            var sb = new StringBuilder();

            foreach (var b in bytesToHexEncode)
            {
                sb.Append(b.ToString("X2"));
            }

            return sb.ToString().ToLower();
        }
        
        #endregion
        
        
        # region Decrypt

        public static TModel DecryptModel<TModel>(string cypherText)
        {
            string plainText = DecryptString(cypherText);
            TModel plainTextModel = JsonConvert.DeserializeObject<TModel>(plainText);
            return plainTextModel;
        }
        
        public static long DecryptId(string cypherText)
        {
            string plainText = DecryptString(cypherText);
            long plainTextId = long.Parse(plainText);
            return plainTextId;
        }

        public static string DecryptString(string hexEncodedCypherText)
        {
            // Base 16 decode
            byte[] cypherTextBytes = HexDecode(hexEncodedCypherText);
            
            // Decrypt
            string plainText = DecryptBytes(cypherTextBytes);

            return plainText;
        }
        
        private static byte[] HexDecode(string hexEncodedString)
        {
            var bytes = new byte[hexEncodedString.Length / 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexEncodedString.Substring(i * 2, 2), 16);
            }

            return bytes;
        }

        private static string DecryptBytes(byte[] cipherTextBytes)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = _keyBytes;
                aesAlg.IV = _ivBytes;

                using (ICryptoTransform decryptor = aesAlg.CreateDecryptor())
                using (MemoryStream memoryStream = new MemoryStream(cipherTextBytes))
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                using (StreamReader streamReader = new StreamReader(cryptoStream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }

        #endregion
        
        
        #region OLD CODE

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

        private static readonly byte[] PrimerBytes = {0, 0, 0, 0};

        //Check the unencrypted data is delimited by the primer bytes
        private static bool WasEncrypted(this byte[] bytes)
        {
            return IsWrapped(bytes, PrimerBytes, PrimerBytes);
        }

        private static byte[] Encrypt(byte[] bytes)
        {
            // Create a encryptor.
            ICryptoTransform encryptor = GetTransform(_defaultEncryptionKey, true);

            //Wrap in the primer bytes so we can later check if the decryption was correct for the key
            bytes = Wrap(bytes, PrimerBytes, PrimerBytes);

            // Return encrypted bytes.
            bytes = CipherStreamWrite(encryptor, bytes);

            return Compress(bytes);
        }

        private static string Encrypt(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return text;
            }

            // Turn input strings into a byte array.
            byte[] bytes = EncryptionEncoding.GetBytes(text);

            // Get encrypted bytes.
            byte[] encryptedBytes = Encrypt(bytes);

            return Convert.ToBase64String(encryptedBytes);
        }

        //[DebuggerStepThrough]
        private static byte[] Decrypt(byte[] encryptedBytes)
        {
            //Ensure the bytes are decompressed
            encryptedBytes = Decompress(encryptedBytes);

            // First, try with encryption primer 
            bool tryWithPrimer = true;
            bool found = DecryptBytes(encryptedBytes, tryWithPrimer, out byte[] decryptedBytes);

            // Try again without encryption primer 
            if (!found)
            {
                tryWithPrimer = false;
                found = DecryptBytes(encryptedBytes, tryWithPrimer, out decryptedBytes);
            }

            if (!found)
            {
                throw new CryptographicException("Could not decrypt using specified keys");
            }

            if (tryWithPrimer)
            {
                decryptedBytes = Strip(decryptedBytes, PrimerBytes.Length, PrimerBytes.Length);
            }

            return decryptedBytes;
        }

        private static bool DecryptBytes(byte[] encryptedBytes, bool tryWithPrimer, out byte[] decryptedBytes)
        {
            var found = false;
            decryptedBytes = null;
            ICryptoTransform decryptor;
            var decrypted = false;
            try
            {
                decryptor = GetTransform(_defaultEncryptionKey, false);

                decryptedBytes = CipherStreamWrite(decryptor, encryptedBytes);

                decryptedBytes = Decompress(decryptedBytes);

                decrypted = true;
            }
            catch (CryptographicException) { }

            if (decrypted && (!tryWithPrimer || decryptedBytes.WasEncrypted()))
            {
                found = true;
            }

            return found;
        }

        [DebuggerStepThrough]
        private static string Decrypt(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return text;
            }

            // Convert Base64 string into a byte array. 
            text = text.Replace("\n", "");
            text = text.Replace("\r", "");
            text = text.Replace(" ", "");
            text = text.Trim();

            byte[] bytes = Convert.FromBase64String(text);

            // Return decrypted string.   
            bytes = Decrypt(bytes);

            return EncryptionEncoding.GetString(bytes);
        }

        private static readonly byte[] GZipHeaderBytes = {0x1f, 0x8b, 8, 0, 0, 0, 0, 0, 4, 0};
        private static readonly byte[] GZipLevel10HeaderBytes = {0x1f, 0x8b, 8, 0, 0, 0, 0, 0, 2, 0};
        private static readonly byte[] GZipLevel12HeaderBytes = {0x1f, 0x8b, 8, 0, 0, 0, 0, 0, 0, 11};

        private static bool IsCompressed(this byte[] bytes)
        {
            if (bytes.Length <= 14)
            {
                return false;
            }

            byte[] header = SubArray(bytes, 4, 10);

            if (header.SequenceEqual(GZipLevel12HeaderBytes)
                || header.SequenceEqual(GZipHeaderBytes)
                || header.SequenceEqual(GZipLevel10HeaderBytes))
            {
                return true;
            }

            header = SubArray(bytes, 0, 10);

            return header.SequenceEqual(GZipLevel12HeaderBytes)
                   || header.SequenceEqual(GZipHeaderBytes)
                   || header.SequenceEqual(GZipLevel10HeaderBytes);
        }

        /// <summary>
        ///     Compresses the string.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        private static byte[] Compress(byte[] buffer, bool mandatory = false)
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
        private static byte[] Decompress(byte[] gZipBuffer)
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

        private static bool IsEncryptedData(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return false;
            }

            return data.StartsWith("===") && data.EndsWith("===");
        }

        private static bool IsPrivateData(this string data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return false;
            }

            if (IsEncryptedData(data))
            {
                data = Decrypt(data.Substring(3, data.Length - 6));
            }

            if (data.StartsWith("##mkPrivatePassword:", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            if (data.StartsWith("mkRecipientPassword:", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }

        [Obsolete]
        public static string EncryptData(string data)
        {
            if (IsEncryptedData(data))
            {
                data = Decrypt(data.Substring(3, data.Length - 6));
            }

            if (string.IsNullOrWhiteSpace(data))
            {
                return null;
            }

            return "===" + Encrypt(data) + "===";
        }

        [Obsolete]
        public static string DecryptData(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return null;
            }

            if (IsEncryptedData(data))
            {
                data = Decrypt(data.Substring(3, data.Length - 6));
            }

            if (data.IsPrivateData())
            {
                return data.Substring(20);
            }

            return data;
        }

        private static bool IsWrapped(byte[] data, byte[] prefix, byte[] suffix)
        {
            if (data.Length < prefix.Length + suffix.Length)
            {
                return false;
            }

            byte[] end = SubArray(data, 0, prefix.Length);

            if (!end.SequenceEqual(prefix))
            {
                return false;
            }

            end = SubArray(data, data.Length - suffix.Length, suffix.Length);

            return end.SequenceEqual(suffix);
        }

        private static byte[] Wrap(byte[] data, byte[] prefix, byte[] suffix)
        {
            var result = new byte[data.Length + prefix.Length + suffix.Length];
            Buffer.BlockCopy(prefix, 0, result, 0, prefix.Length);
            Buffer.BlockCopy(data, 0, result, prefix.Length, data.Length);
            Buffer.BlockCopy(suffix, 0, result, prefix.Length + data.Length, suffix.Length);
            return result;
        }

        private static byte[] Strip(byte[] data, int left, int right)
        {
            var result = new byte[data.Length - (left + right)];
            Buffer.BlockCopy(data, left, result, 0, result.Length);
            return result;
        }

        private static byte[] SubArray(byte[] data, int index, int length)
        {
            if (length > data.Length)
            {
                length = data.Length;
            }

            var result = new byte[length];
            Buffer.BlockCopy(data, index, result, 0, length);
            return result;
        }

        #endregion

    }
}
