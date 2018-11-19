using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Mechanisms.Extensions;

// ReSharper disable ParameterTypeCanBeEnumerable.Local

namespace AnsVaultCmd
{
    public static class Decrypter
    {
        public static string Decypt(IEnumerable<string> input, string password)
        {
            var file = input.ToArray();
            var header = file.First();
            var fields = header.Split(';');

            if (fields.Length != 3)
                throw new InvalidVaultException("Unsupported file header field count: {0}".Fmt(fields.Length));

            if (fields[0] != "$ANSIBLE_VAULT")
                throw new InvalidVaultException("Unsupported type in file header: {0}".Fmt(fields[0]));

            if (fields[1] != "1.1")
                throw new InvalidVaultException("Unsupported version in file header: {0}".Fmt(fields[1]));

            if (fields[2] != "AES256")
                throw new InvalidVaultException("Unsupported cipher in file header: {0}".Fmt(fields[1]));

            var hexPayload = string.Join("", file.Skip(1).ToArray());
            var binPayload = Encoding.UTF8.GetString(FromHex(hexPayload));
            var payloadFields = binPayload.Split('\n');

            var salt = FromHex(payloadFields[0]);
            var hmac = FromHex(payloadFields[1]);
            var ciphertext = FromHex(payloadFields[2]);

            const int cipherKeyLength = 32;
            const int hmacKeyLength = 32;
	        const int ivLength = 16;
            const int pbkdf2KeyLength = cipherKeyLength + hmacKeyLength + ivLength;
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var pbkdf2Key = PBKDF2Sha256.GetBytes(pbkdf2KeyLength, passwordBytes, salt, 10000);
            var cipherKey = new byte[cipherKeyLength];
            Array.Copy(pbkdf2Key, 0, cipherKey, 0, cipherKeyLength);    
            var hmacKey = new byte[hmacKeyLength];
            Array.Copy(pbkdf2Key, cipherKeyLength, hmacKey, 0, hmacKeyLength);
            var cipherIv = new byte[ivLength];
            Array.Copy(pbkdf2Key, cipherKeyLength+hmacKeyLength, cipherIv, 0, ivLength);

            if (!CheckMac(hmacKey, ciphertext, hmac))
                throw new InvalidMacException("The MACs did not match; invalid password?");

            var cipher = new Aes128CounterMode(cipherIv);
            var decrypter = cipher.CreateDecryptor(cipherKey, null);
            var outputBytes = decrypter.TransformFinalBlock(ciphertext, 0, ciphertext.Length);
            var padding = outputBytes[outputBytes.Length-1];
            var plaintext = Encoding.UTF8.GetString(outputBytes, 0, outputBytes.Length - padding);

            return plaintext;
        }

        internal static byte[] FromHex(string hex)
        {
            var numBytes = hex.Length / 2;
            var binary = new byte[numBytes];

            for (int h = 0, b = 0; h != hex.Length; h += 2, b += 1)
            {
                var hi = DigitToByte(hex[h+0]);
                var lo = DigitToByte(hex[h+1]);

                binary[b] = (byte)((hi << 4) + lo);
            }

            return binary;
        }

        private static byte DigitToByte(char digit)
        {
            if (digit >= '0' && digit <= '9')
                return (byte)(digit - '0');

            if (digit >= 'A' && digit <= 'F')
                return (byte)(digit - 'A' + 10);

            if (digit >= 'a' && digit <= 'f')
                return (byte)(digit - 'a' + 10);

            throw new ArgumentOutOfRangeException("digit", "'{0}' is not a hex digit".Fmt(digit));
        }

        private static bool CheckMac(byte[] key, byte[] message, byte[] expectedHash)
        {
            var hmac = new HMACSHA256(key);
            var hash = hmac.ComputeHash(message);

            return hash.SequenceEqual(expectedHash);
        }
    }
}
