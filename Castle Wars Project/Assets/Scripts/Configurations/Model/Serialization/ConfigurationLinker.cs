using Configurations.Model.Id;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Configurations.Model.Serialization
{
    public static class ConfigurationLinker
    {
        private static readonly Dictionary<Type, bool> _typeAnalysisCache = new();

        public static void LinkConfigurationReferences(Dictionary<ConfigurationId, GameConfiguration> configurations)
        {
            foreach (var config in configurations.Values)
            {
                if (!HasNestedConfigs(config.GetType()))
                    continue;
                LinkReferencesRecursive(config, configurations);
            }
        }

        private const BindingFlags AllInstanceFields = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        public static void LinkReferencesRecursive(object target, Dictionary<ConfigurationId, GameConfiguration> configurations, HashSet<object> visited = null)
        {
            if (target == null)
                return;

            var type = target.GetType();

            // Primitive types, enums and strings are not containers for configs
            if (type.IsPrimitive || type.IsEnum || target is string)
                return;

            // We only track reference types for circular dependencies. 
            // Structs are copied, so they don't cause infinite recursion in the same way.
            if (!type.IsValueType)
            {
                visited ??= new HashSet<object>(ReferenceEqualityComparer.Instance);
                if (!visited.Add(target))
                    return;
            }

            // --- 1. Handling Collections ---
            if (target is IList list)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var item = list[i];
                    if (item == null) continue;

                    if (item is GameConfiguration stub && configurations.TryGetValue(stub.id, out var real))
                    {
                        list[i] = real;
                    }
                    else
                    {
                        // If it's a struct inside a list, we MUST re-assign it after modification
                        if (item.GetType().IsValueType)
                        {
                            LinkReferencesRecursive(item, configurations, visited);
                            list[i] = item; // Write back modified boxed struct
                        }
                        else
                        {
                            LinkReferencesRecursive(item, configurations, visited);
                        }
                    }
                }
                return;
            }

            if (!HasNestedConfigs(type))
                return;

            var fields = type.GetFields(AllInstanceFields);
            foreach (var field in fields)
            {
                var fieldValue = field.GetValue(target);
                if (fieldValue == null)
                    continue;

                if (typeof(GameConfiguration).IsAssignableFrom(field.FieldType))
                {
                    var stubConfig = (GameConfiguration)fieldValue;
                    if (configurations.TryGetValue(stubConfig.id, out var realConfig))
                    {
                        field.SetValue(target, realConfig);
                    }
                }
                else if (HasNestedConfigs(field.FieldType))
                {
                    LinkReferencesRecursive(fieldValue, configurations, visited);
                }
            }
        }

        // Optimization: check if type can even contain a BaseConfiguration
        public static bool HasNestedConfigs(Type type)
        {
            if (type.IsPrimitive || type == typeof(string) || type.IsEnum)
                return false;

            if (_typeAnalysisCache.TryGetValue(type, out bool result))
                return result;

            _typeAnalysisCache[type] = true; // temporary to avoid recursion issues

            // 1. Check if it's an array
            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                if (typeof(GameConfiguration).IsAssignableFrom(elementType))
                {
                    return _typeAnalysisCache[type] = true;
                }
                return _typeAnalysisCache[type] = HasNestedConfigs(elementType);
            }

            // 2. Check if it's IEnumerable or IEnumerable<T>
            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                // Try to get T from IEnumerable<T>
                if (type.IsGenericType)
                {
                    return _typeAnalysisCache[type] = HasNestedConfigs(type.GetGenericArguments()[0]);
                }
                else
                {
                    // Fallback for non-generic collections like ArrayList
                    return _typeAnalysisCache[type] = false;
                }
            }

            // 3. Check fields for complicated types
            var fields = type.GetFields(AllInstanceFields);
            foreach (var field in fields)
            {
                if (typeof(GameConfiguration).IsAssignableFrom(field.FieldType))
                    return _typeAnalysisCache[type] = true;

                if (HasNestedConfigs(field.FieldType))
                    return _typeAnalysisCache[type] = true;
            }
            return _typeAnalysisCache[type] = false;
        }
        
        // Helper for HashSet to compare by reference only
        internal class ReferenceEqualityComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceEqualityComparer Instance = new();
            public new bool Equals(object x, object y) => ReferenceEquals(x, y);
            public int GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }
    }
}