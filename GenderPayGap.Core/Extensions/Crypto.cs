using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using GenderPayGap.Core;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace GenderPayGap.Extensions
{
    public static class Crypto
    {

        public static string GetPBKDF2(string password,
            byte[] salt,
            KeyDerivationPrf prfunction = KeyDerivationPrf.HMACSHA1,
            int iterationCount = 10000,
            int hashSizeInBit = 256)
        {
            return Convert.ToBase64String(
                KeyDerivation.Pbkdf2(
                    password,
                    salt,
                    prfunction,
                    iterationCount,
                    hashSizeInBit / 8));
        }

        public static byte[] GetSalt(int saltSizeInBit = 128)
        {
            // generate a salt using a secure PRNG
            var salt = new byte[saltSizeInBit / 8];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            return salt;
        }


        public static string GetSHA512Checksum(string text, bool base64encode = true)
        {
            byte[] checksumData = Encoding.UTF8.GetBytes(text);
            byte[] hash = SHA512.Create().ComputeHash(checksumData);
            string calculatedChecksum = base64encode ? Convert.ToBase64String(hash) : Encoding.UTF8.GetString(hash);
            return calculatedChecksum;
        }

        public static string GeneratePinInThePost()
        {
            string pin = "";

            while (pin.Length < 7)
            {
                int randomIndex = new Random().Next(Global.PINChars.Length);
                char nextCharacter = Global.PINChars[randomIndex];
                
                pin += nextCharacter;
            }
            
            return pin;
        }

    }
}
