using System;
using System.Web;
using GenderPayGap.Extensions;

namespace GenderPayGap.Core.Classes
{
    /// <summary>
    ///     This class exists as a response to code that encrypted id's and created some long URL strings. Please do not use,
    ///     search instead for IObfuscator in this solution.
    /// </summary>
    [Obsolete("Please use other classes that implement IObfuscator")]
    public interface IEncryptionHandler
    {

        string EncryptAndEncode(long valueToEncryptAndEncode);
        string DecryptAndDecode(string valueToDecryptAndDecode);

    }

    public class EncryptionHandler : IEncryptionHandler
    {

        public string EncryptAndEncode(long valueToEncryptAndEncode)
        {
            return EncryptAndEncode(valueToEncryptAndEncode.ToInt32());
        }

        public string DecryptAndDecode(string valueToDecryptAndDecode)
        {
            return HttpUtility.UrlDecode(Encryption.DecryptQuerystring(valueToDecryptAndDecode));
        }

        private string EncryptAndEncode(int valueToEncryptAndEncode)
        {
            return Encryption.EncryptQuerystring(valueToEncryptAndEncode.ToString());
        }

    }
}
