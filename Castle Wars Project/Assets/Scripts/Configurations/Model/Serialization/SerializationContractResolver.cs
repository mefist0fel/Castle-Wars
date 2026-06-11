using Configurations.Model.Id;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Configurations.Model.Serialization
{
    public sealed class SerializationContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);

            if (!prop.Writable && member is FieldInfo field && field.IsInitOnly)
            {
                prop.Writable = true; // Force readonly fields to be writable
            }
            return prop;
        }

        protected override List<MemberInfo> GetSerializableMembers(Type objectType)
        {
            return base.GetSerializableMembers(objectType);
        }

        // Newtonsoft.Json uses contract.Converter (not SerializerSettings.Converters) when
        // deserialising dictionary keys. Setting the converter on the contract itself ensures
        // ConfigurationId is handled correctly both as a value and as a dictionary key.
        public override JsonContract ResolveContract(Type type)
        {
            var contract = base.ResolveContract(type);
            if (type == typeof(ConfigurationId) && contract.Converter == null)
                contract.Converter = new ConfigurationIdJsonConverter();
            return contract;
        }
    }
}