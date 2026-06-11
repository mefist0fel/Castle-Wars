using Configurations.Model;
using Configurations.Model.Id;
using System;
using UnityEngine;

namespace Configurations
{
    [Serializable]
    public class ConfigurationField
    {
        [SerializeField]
        private ConfigurationContainer container = null;
        [SerializeField]
        private int id;
        [NonSerialized]
        private GameConfiguration value = null;

        public T Get<T>() where T : GameConfiguration
        {
#if UNITY_EDITOR
            if (value != null && value.Id != id)
            {
                value = null;
            }
#endif
            if (value == null && container != null && id != ConfigurationId.Invalid)
            {
                value = container.Configuration.Get(id);
            }
            return value as T;
        }

        public GameConfiguration GetValue()
        {
#if UNITY_EDITOR
            if (value != null && value.Id != id)
            {
                value = null;
            }
#endif
            if (value == null || value.Id == ConfigurationId.Invalid && container != null && id != ConfigurationId.Invalid)
            {
                value = container.Configuration.Get(id);
            }
            return value;
        }

        public void SetValue(GameConfiguration config)
        {
            value = config;
            if (config != null)
            {
                id = (int)config.id;
            }
            else
            {
                id = (int)ConfigurationId.Invalid;
            }
        }
    }

    [Serializable]
    public class ConfigurationField<T> : ConfigurationField where T : GameConfiguration
    {
        public T Value => Get<T>();

        public T Get() => (T)base.Get<T>();
    }
}