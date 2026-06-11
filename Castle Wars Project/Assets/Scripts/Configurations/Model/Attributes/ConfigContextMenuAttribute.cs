using System;

namespace Configurations.Model
{

    /// <summary>
    /// Adds this method as an item in the right-click context menu of the config inspector.
    /// Only supported on parameterless void methods.
    /// The method may mutate the config object directly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class ConfigContextMenuAttribute : Attribute
    {
        public string Label { get; }

        public ConfigContextMenuAttribute(string label)
        {
            Label = label;
        }
    }
}
