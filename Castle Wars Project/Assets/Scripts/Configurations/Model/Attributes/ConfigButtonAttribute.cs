using System;

namespace Configurations.Model
{
    /// <summary>
    /// Renders a clickable button in the config inspector that invokes this method.
    /// Only supported on parameterless void methods.
    /// The method may mutate the config object directly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ConfigButtonAttribute : Attribute
    {
        /// <summary>Button label. If null, the method name is used.</summary>
        public string Label { get; }

        public ConfigButtonAttribute(string label = null)
        {
            Label = label;
        }
    }
}
