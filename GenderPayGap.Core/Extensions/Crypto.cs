using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
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

        public static string GeneratePasscode(char[] charset, int passcodeLength)
        {
            //Ensure characters are distict and mixed up
            charset = charset.Distinct().ToList().Randomise().ToArray();

            var chars = new char[passcodeLength];

            //Generate a load of random numbers
            var randomData = new byte[chars.Length];
            using (var generator = new RNGCryptoServiceProvider())
            {
                generator.GetBytes(randomData);
            }

            //use the randome number to pick from the character set
            Parallel.For(0, chars.Length, i => { chars[i] = charset[randomData[i] % charset.Length]; });

            return new string(chars);
        }

    }
}
