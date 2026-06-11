using Configurations.Model.Id;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Configurations.Model
{
    [System.Serializable]
    public class GameConfiguration :
        IEquatable<GameConfiguration>,
        IComparable<GameConfiguration>
    {
        [JsonProperty("id")]
        public ConfigurationId id;

        [JsonProperty("name")]
        public readonly string name;

        [JsonIgnore]
        public ConfigurationId Id => id;
        [JsonIgnore]
        public string Name => name;

        public GameConfiguration(ConfigurationId id = default, string name = null)
        {
            this.id = id;
            this.name = name;
        }

        #region Equality Comparison
        public bool Equals(GameConfiguration other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return EqualityComparer<ConfigurationId>.Default.Equals(id, other.id);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((GameConfiguration)obj);
        }

        public override int GetHashCode() =>
            (id != null ? id.GetHashCode() : 0);

        public static bool operator ==(GameConfiguration left, GameConfiguration right) =>
            Equals(left, right);

        public static bool operator !=(GameConfiguration left, GameConfiguration right) =>
            !Equals(left, right);

        public int CompareTo(GameConfiguration other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (other is null) return 1;

            return Comparer<ConfigurationId>.Default.Compare(id, other.id);
        }
        #endregion
    }
}