using System.Linq;
using Mechanisms.Tests;
using VaultCmd;

namespace Tests
{
    public static class DecryptionTests
    {
        [TestCases]
        public static void decryption()
        {
            "Decrypting with the correct key returns the original text".Is(() =>
            {
                var ciphertext = ValidHeader.Concat(ValidCiphertext);
                var plaintext = "TEST";

                var output = Decrypter.Decypt(ciphertext, ValidPassword);

                Assert.True(output == plaintext);
            });

            "Decryption fails when the header does not have 3 fields".Is(() =>
            {
                var invalidHeader = new[]
                {
                    "$ANSIBLE_VAULT;AES256",
                };

                var ciphertext = invalidHeader.Concat(ValidCiphertext);

                Assert.Throws(() => Decrypter.Decypt(ciphertext, ValidPassword));
            });

            "Decryption fails when not an ansible vault file".Is(() =>
            {
                var invalidHeader = new[]
                {
                    "$SOME_OTHER_FILE;1.1;AES256",
                };

                var ciphertext = invalidHeader.Concat(ValidCiphertext);

                Assert.Throws(() => Decrypter.Decypt(ciphertext, ValidPassword));
            });

            "Decryption fails when not the correct version".Is(() =>
            {
                var invalidHeader = new[]
                {
                    "$ANSIBLE_VAULT;0.0;AES256",
                };

                var ciphertext = invalidHeader.Concat(ValidCiphertext);

                Assert.Throws(() => Decrypter.Decypt(ciphertext, ValidPassword));
            });

            "Decryption fails when not the correct cipher".Is(() =>
            {
                var invalidHeader = new[]
                {
                    "$ANSIBLE_VAULT;1.1;BOB123",
                };

                var ciphertext = invalidHeader.Concat(ValidCiphertext);

                Assert.Throws(() => Decrypter.Decypt(ciphertext, ValidPassword));
            });

            "Decrypting with the correct key returns the original text".Is(() =>
            {
                const string invalidPassword = "not-the-password";
                var ciphertext = ValidHeader.Concat(ValidCiphertext);

                Assert.Throws(() => Decrypter.Decypt(ciphertext, invalidPassword));
            });
        }

        [TestCases]
        public static void hex_to_binary_conversion()
        {
            "Pairs of digits equate to a single byte".Is(() =>
            {
                const string hex = "0123456789ABCDEF";

                var binary = Decrypter.FromHex(hex);

                Assert.True(binary.SequenceEqual(new byte[]{0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF}));
            });

            "Non-hex digits generate an exception".Is(() =>
            {
                const string hex = "ZZ";

                Assert.Throws(() => Decrypter.FromHex(hex));
            });

            "The hex stream must have an even number of digits".Is(() =>
            {
                const string hex = "F";

                Assert.Throws(() => Decrypter.FromHex(hex));
            });
        }

        private static readonly string[] ValidHeader = new[]
        {
            "$ANSIBLE_VAULT;1.1;AES256",
        };

        private static readonly string[] ValidCiphertext = new[]
        { 
            "65306664396363623635396463366664353130646532316462343063336536623663306432386637",
            "3632363837373131646265363639336132316637326534660a386438343963666565373361376163",
            "37396630366665633332663331303963363836316632363664336339663134326465383630363433",
            "6161336634656661370a353331666363373233373464316138376336356339366335663063653035",
            "6162"
        };

        private const string ValidPassword = "password";
    }
}
