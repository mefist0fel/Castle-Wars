using System.Text;

namespace Configurations.Model.Id
{
    public static class StringExtensions
    {
        public static int GetStableHash(this string input) => MurmurHash3(input);

        public static int MurmurHash3(this string input)
        {
            if (input == null) return 0;

            const uint seed = 1442682977;
            const uint c1 = 0xcc9e2d51;
            const uint c2 = 0x1b873593;

            byte[] data = Encoding.UTF8.GetBytes(input);
            int length = data.Length;
            uint hash = seed;
            int currentIndex = 0;

            while (length >= 4)
            {
                uint k = (uint)(data[currentIndex] | data[currentIndex + 1] << 8 | data[currentIndex + 2] << 16 | data[currentIndex + 3] << 24);
                k *= c1;
                k = k << 15 | k >> 17;
                k *= c2;

                hash ^= k;
                hash = hash << 13 | hash >> 19;
                hash = hash * 5 + 0xe6546b64;

                currentIndex += 4;
                length -= 4;
            }

            uint tail = 0;
            switch (length)
            {
                case 3: tail ^= (uint)data[currentIndex + 2] << 16; goto case 2;
                case 2: tail ^= (uint)data[currentIndex + 1] << 8; goto case 1;
                case 1:
                    tail ^= data[currentIndex];
                    tail *= c1;
                    tail = tail << 15 | tail >> 17;
                    tail *= c2;
                    hash ^= tail;
                    break;
            }

            hash ^= (uint)data.Length;
            hash ^= hash >> 16;
            hash *= 0x85ebca6b;
            hash ^= hash >> 13;
            hash *= 0xc2b2ae35;
            hash ^= hash >> 16;

            return (int)hash;
        }
    }
}