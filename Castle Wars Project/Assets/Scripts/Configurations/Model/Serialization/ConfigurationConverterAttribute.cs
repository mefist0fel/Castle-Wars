using System;

namespace Configurations.Model.Serialization
{
    /// <summary>
    /// Marks a <see cref="Newtonsoft.Json.JsonConverter"/> subclass for auto-discovery
    /// by the configuration system. All non-abstract converters with this attribute are
    /// instantiated and added to <see cref="ConfigurationContainer.SerializerSettings"/>
    /// at startup by scanning all loaded assemblies.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class ConfigurationConverterAttribute : Attribute { }
}
