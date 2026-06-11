#if UNITY_EDITOR
using System;
using Configurations.Model;
using Configurations.Model.Serialization;
using Newtonsoft.Json;
using UnityEngine;

namespace Configurations.Editor
{
    /// <summary>
    /// Clipboard for copy-pasting field values inside configuration editors.
    /// GameConfiguration references are copied by pointer (not cloned).
    /// All other types are serialized to JSON.
    /// </summary>
    internal static class ConfigurationClipboard
    {
        private static string _json;
        private static Type _storedType;
        private static GameConfiguration _configRef; // direct ref for GameConfiguration fields

        public static bool HasValue => _storedType != null;

        public static void Copy(object value, Type type)
        {
            _storedType = type;
            _json = null;
            _configRef = null;

            if (value == null)
            {
                _json = "null";
                return;
            }

            // GameConfiguration references: copy by pointer, no JSON needed
            if (typeof(GameConfiguration).IsAssignableFrom(type))
            {
                _configRef = value as GameConfiguration;
                return;
            }

            // Everything else: JSON with type info so polymorphic types round-trip correctly
            try
            {
                _json = JsonConvert.SerializeObject(value, type, ClipboardSettings);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Clipboard] Copy failed for {type.Name}: {e.Message}");
                _storedType = null;
            }
        }

        public static bool CanPaste(Type targetType)
        {
            if (_storedType == null)
                return false;
            return targetType == _storedType
                || targetType.IsAssignableFrom(_storedType)
                || _storedType.IsSubclassOf(targetType);
        }

        public static bool TryPaste(Type targetType, out object result)
        {
            result = null;
            if (!CanPaste(targetType))
                return false;

            // GameConfiguration: return stored reference directly
            if (typeof(GameConfiguration).IsAssignableFrom(_storedType))
            {
                result = _configRef;
                return true;
            }

            if (_json == null)
                return false;

            try
            {
                result = JsonConvert.DeserializeObject(_json, targetType, ClipboardSettings);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Clipboard] Paste failed for {targetType.Name}: {e.Message}");
                return false;
            }
        }

        public static string StoredTypeName => _storedType?.Name ?? "—";

        // Uses same converters as ConfigurationContainer but without GameConfigurationJsonConverter
        // (GameConfiguration fields are handled by reference, not JSON)
        private static readonly JsonSerializerSettings ClipboardSettings = new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Include,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            Converters = { new ConfigurationIdJsonConverter() },
            SerializationBinder = new AllAssembliesSerializationBinder(),
            ContractResolver = new SerializationContractResolver(),
        };
    }
}
#endif
