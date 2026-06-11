using System;
using System.ComponentModel;
using System.Globalization;

namespace Configurations.Model.Id
{
    [System.Serializable]
    [TypeConverter(typeof(ConfigurationIdTypeConverter))]
    public readonly struct ConfigurationId :
        IEquatable<ConfigurationId>, IComparable<ConfigurationId>, IComparable
    {
        public readonly string Id => _id.ToString("X");
        private readonly int _id;

        public static readonly ConfigurationId Invalid = new(0);

        public ConfigurationId(int id = 0)
        {
            _id = id;
        }

        public ConfigurationId(string hexId)
        {
            if (string.IsNullOrEmpty(hexId))
            {
                _id = 0;
                return;
            }

            // Remove "0x" prefix if present
            if (hexId.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                hexId = hexId.Substring(2);
            }

            if (int.TryParse(hexId, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int result))
            {
                _id = result;
            }
            else
            {
                _id = 0;
            }
        }

        public override readonly string ToString() => _id.ToString("X");

        public static ConfigurationId NewId()
        {
            byte[] guidBytes = Guid.NewGuid().ToByteArray();
            int randomInt = BitConverter.ToInt32(guidBytes, 0);

            return new ConfigurationId(randomInt);
        }

        #region Comparison

        public bool Equals(ConfigurationId other) => _id == other._id;
        public override bool Equals(object obj) => obj is ConfigurationId other && Equals(other);

        public override int GetHashCode() => _id;

        public int CompareTo(ConfigurationId other) => _id.CompareTo(other._id);
        public int CompareTo(object obj) => obj is ConfigurationId other ? CompareTo(other) : throw new ArgumentException("Object is not a ConfigurationId");

        public static bool operator ==(ConfigurationId left, ConfigurationId right) => left._id == right._id;
        public static bool operator !=(ConfigurationId left, ConfigurationId right) => left._id != right._id;
        #endregion

        #region Conversions
        public static explicit operator int(ConfigurationId identifier) => identifier._id;
        public static implicit operator ConfigurationId(int id) => new(id);
        #endregion
    }
}