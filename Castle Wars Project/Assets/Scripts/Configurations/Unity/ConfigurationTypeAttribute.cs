using System;
using UnityEngine;

namespace Configurations
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class)]
    public class ConfigurationTypeAttribute : PropertyAttribute
    {
        public Type TargetType { get; private set; }

        public ConfigurationTypeAttribute(Type targetType)
        {
            TargetType = targetType;
        }
    }
}