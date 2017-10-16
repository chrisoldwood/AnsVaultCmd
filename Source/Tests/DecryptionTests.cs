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
                const string password = "password";

                var ciphertext = new[]
                { 
                    "$ANSIBLE_VAULT;1.1;AES256",
                    "65633232376535306266643337656230373531306536303761353166343434643231636664313339",
                    "3365663032646265386239643939656636376361623863360a303737663737306231363039346366",
                    "37643430383131313464616637396461653838343738363531633339613430386533663232346366",
                    "3964616130336639390a313234643537613163636133346366393435343463643530626139303538",
                    "62623665383866386664316431343834346265313564643438663532633961623562",
                };

                var plaintext = new[]
                {
                    "test_key: \"test_value\"",
                    "",
                };

                var output = Decrypter.Decypt(ciphertext, password).ToArray();

                Assert.True(output.Length == plaintext.Length);

                for (int line = 0; line != plaintext.Length; ++line)
                    Assert.True(output[line] == plaintext[line]);
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
    }
}
