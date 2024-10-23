namespace GenderPayGap.Core.Classes
{
    public static class Obfuscator
    {

        public static int DeObfuscate(string value)
        {
            var obfuscator = new Cryptography.Obfuscation.Obfuscator { Seed = Global.ObfuscationSeed };
            return obfuscator.Deobfuscate(value);
        }

    }
}
