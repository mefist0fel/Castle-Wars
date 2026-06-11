using Configurations.Model.Id;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Configurations.Model
{
    public sealed class Configuration : IEnumerable<GameConfiguration>
    {
        private readonly Dictionary<ConfigurationId, GameConfiguration> configs;
        public Dictionary<ConfigurationId, GameConfiguration> Configurations => configs;

        public Dictionary<Type, GameConfiguration> instances = new();

        public T GetInstance<T>() where T : GameConfiguration
        {
            if (instances.ContainsKey(typeof(T)))
            {
                return instances[typeof(T)] as T;
            }
            var instance = configs.FirstOrDefault(item => item.GetType() == typeof(T)).Value;
            instances.Add(typeof(T), instance);
            return instance as T;
        }

        public T Get<T>(ConfigurationId id) where T : GameConfiguration =>
            configs[id] as T;

        public GameConfiguration Get(ConfigurationId id) =>
            configs[id];

        public Configuration(Dictionary<ConfigurationId, GameConfiguration> configs = null)
        { 
            this.configs = configs ?? new Dictionary<ConfigurationId, GameConfiguration>();
        }

        public IEnumerator<GameConfiguration> GetEnumerator() => configs.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}