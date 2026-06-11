using System;

namespace Configurations.Model.Id
{
    public static class ConfigurationIdGenerator
    {
        private static readonly char[] Base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567".ToCharArray();

        public static string GenerateShortGuid()
        {
            byte[] guidBytes = Guid.NewGuid().ToByteArray();
            return Base32Encode(guidBytes).Substring(0, 8);
        }

        private static string Base32Encode(byte[] bytes)
        {
            int bitBuffer = 0, bitCount = 0, index = 0;
            char[] result = new char[(bytes.Length * 8 + 4) / 5];

            foreach (byte b in bytes)
            {
                bitBuffer = bitBuffer << 8 | b;
                bitCount += 8;

                while (bitCount >= 5)
                {
                    result[index++] = Base32Chars[bitBuffer >> bitCount - 5 & 0x1F];
                    bitCount -= 5;
                }
            }

            if (bitCount > 0)
            {
                result[index++] = Base32Chars[bitBuffer << 5 - bitCount & 0x1F];
            }

            return new string(result, 0, index);
        }
    }
}