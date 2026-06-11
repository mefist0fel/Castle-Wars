using System;

namespace Configurations.Model
{
    /// <summary>
    /// Draws a bold section label above this field in the config inspector.
    /// Use to visually separate groups of fields.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ConfigHeaderAttribute : Attribute
    {
        public string Label { get; }

        public ConfigHeaderAttribute(string label)
        {
            Label = label;
        }
    }
}
