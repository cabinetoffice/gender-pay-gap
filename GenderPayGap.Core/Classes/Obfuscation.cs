using GenderPayGap.Extensions;

namespace GenderPayGap.Core.Classes
{
    public static class Obfuscator
    {

        public static string Obfuscate(long value)
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
