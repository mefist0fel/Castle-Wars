using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Serialization;

namespace Configurations.Model.Serialization
{
    public sealed class AllAssembliesSerializationBinder : ISerializationBinder
    {
        private static readonly Dictionary<string, Type> _typeCache = new();
        private static readonly List<Type> _allTypes =
            AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes()).ToList();

        public Type BindToType(string assemblyName, string typeName)
        {
            if (_typeCache.TryGetValue(typeName, out var cachedType))
            {
                return cachedType;
            }

            Type type = _allTypes.FirstOrDefault(t => t.FullName == typeName);

            _typeCache[typeName] = type;
            return type;
        }

        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            typeName = serializedType.FullName;
        }
    }
}