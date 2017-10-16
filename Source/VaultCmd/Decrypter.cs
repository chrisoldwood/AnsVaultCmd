using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Mechanisms.Extensions;

namespace VaultCmd
{
    public static class Decrypter
    {
        public static IEnumerable<string> Decypt(IEnumerable<string> input, string password)
        {
            var header = input.First();
            var fields = header.Split(';');

            var hexPayload = string.Join("", input.Skip(1).ToArray());
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
            var pbkdf2Key = PBKDF2Sha256GetBytes(pbkdf2KeyLength, passwordBytes, salt, 10000);
            var cipherKey = new byte[cipherKeyLength];
            Array.Copy(pbkdf2Key, 0, cipherKey, 0, cipherKeyLength);    
            var hmacKey = new byte[hmacKeyLength];
            Array.Copy(pbkdf2Key, cipherKeyLength, hmacKey, 0, hmacKeyLength);
            var cipherIv = new byte[ivLength];
            Array.Copy(pbkdf2Key, cipherKeyLength+hmacKeyLength, cipherIv, 0, ivLength);
            /*var matches =*/ CheckMac(hmacKey, ciphertext, hmac);

            var cipher = new Aes128CounterMode(cipherIv);
            var decrypter = cipher.CreateDecryptor(cipherKey, null);
            var outputBytes = decrypter.TransformFinalBlock(ciphertext, 0, ciphertext.Length);
            var padding = outputBytes[outputBytes.Length-1];
            var outputText = Encoding.UTF8.GetString(outputBytes, 0, outputBytes.Length - padding);

            return outputText.Split(new[]{ "\r\n" }, StringSplitOptions.None);
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

        // From StackOverflow: Rfc2898 / PBKDF2 with SHA256 as digest in c#
        // https://stackoverflow.com/a/18649357/106119
        public static byte[] PBKDF2Sha256GetBytes(int dklen, byte[] password, byte[] salt, int iterationCount){
            using(var hmac=new System.Security.Cryptography.HMACSHA256(password)){
                int hashLength=hmac.HashSize/8;
                if((hmac.HashSize&7)!=0)
                    hashLength++;
                int keyLength=dklen/hashLength;
                if((long)dklen>(0xFFFFFFFFL*hashLength) || dklen<0)
                    throw new ArgumentOutOfRangeException("dklen");
                if(dklen%hashLength!=0)
                    keyLength++;
                byte[] extendedkey=new byte[salt.Length+4];
                Buffer.BlockCopy(salt,0,extendedkey,0,salt.Length);
                using(var ms=new System.IO.MemoryStream()){
                    for(int i=0;i<keyLength;i++){
                        extendedkey[salt.Length]=(byte)(((i+1)>>24)&0xFF);
                        extendedkey[salt.Length+1]=(byte)(((i+1)>>16)&0xFF);
                        extendedkey[salt.Length+2]=(byte)(((i+1)>>8)&0xFF);
                        extendedkey[salt.Length+3]=(byte)(((i+1))&0xFF);
                        byte[] u=hmac.ComputeHash(extendedkey);
                        Array.Clear(extendedkey,salt.Length,4);
                        byte[] f=u;
                        for(int j=1;j<iterationCount;j++){
                            u=hmac.ComputeHash(u);
                            for(int k=0;k<f.Length;k++){
                                f[k]^=u[k];
                            }
                        }
                        ms.Write(f,0,f.Length);
                        Array.Clear(u,0,u.Length);
                        Array.Clear(f,0,f.Length);
                    }
                    byte[] dk=new byte[dklen];
                    ms.Position=0;
                    ms.Read(dk,0,dklen);
                    ms.Position=0;
                    for(long i=0;i<ms.Length;i++){
                        ms.WriteByte(0);
                    }
                    Array.Clear(extendedkey,0,extendedkey.Length);
                    return dk;
                }
            }
        }    
    }
}
