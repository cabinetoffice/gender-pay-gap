using GenderPayGap.Extensions;

namespace GenderPayGap.Core.Classes
{
    public interface IObfuscator
    {

        string Obfuscate(string value);
        string Obfuscate(int value);
        int DeObfuscate(string value);

    }

    public class InternalObfuscator : IObfuscator
    {

        private static readonly int Seed = Global.ObfuscationSeed;
        private readonly Cryptography.Obfuscation.Obfuscator _obfuscator;

        public InternalObfuscator()
        {
            _obfuscator = new Cryptography.Obfuscation.Obfuscator { Seed = Seed};
        }

        public string Obfuscate(string value)
        {
            return Obfuscate(value.ToInt32());
        }

        public string Obfuscate(int value)
        {
            return _obfuscator.Obfuscate(value); // e.g. xVrAndNb
        }

        public int DeObfuscate(string value)
        {
            return _obfuscator.Deobfuscate(value); // 15
        }

    }

    public static class Obfuscator
    {

        public static string Obfuscate(string value)
        {
            return Obfuscate(value.ToInt32());
        }

        public static string Obfuscate(int value)
        {
            var obfuscator = new Cryptography.Obfuscation.Obfuscator { Seed = Global.ObfuscationSeed };
            return obfuscator.Obfuscate(value); // e.g. xVrAndNb
        }

        public static int DeObfuscate(string value)
        {
            var obfuscator = new Cryptography.Obfuscation.Obfuscator { Seed = Global.ObfuscationSeed };
            return obfuscator.Deobfuscate(value); // 15
        }

    }
}
