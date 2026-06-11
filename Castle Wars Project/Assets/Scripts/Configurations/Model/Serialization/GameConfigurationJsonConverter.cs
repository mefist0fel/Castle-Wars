using Configurations.Model.Id;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Configurations.Model.Serialization
{
    /// <summary>
    /// Serializes GameConfiguration objects:
    ///   - root level  → full object (all fields)
    ///   - nested refs → hex ID string only (resolved later by ConfigurationLinker)
    ///
    /// Uses ThreadStatic depth tracking so the single converter instance shared
    /// across the static SerializerSettings works correctly across multiple
    /// consecutive SerializeObject / DeserializeObject calls.
    /// </summary>
    public sealed class GameConfigurationJsonConverter : JsonConverter
    {
        // Tracks how many WriteJson frames are on the stack for this thread.
        // depth == 0 means we are serializing the root config (write all fields).
        // depth > 0 means we are inside a nested object (write ID only).
        [ThreadStatic]
        private static int _writeDepth;

        // Set to true before calling serializer.Serialize() from WriteJson so that
        // CanWrite returns false for that one re-entrant call, letting the default
        // serializer handle the actual field writing without going back into WriteJson.
        [ThreadStatic]
        private static bool _bypassWrite;

        // Same bypass pattern for ReadJson to avoid re-entrant infinite loops.
        [ThreadStatic]
        private static bool _bypassRead;

        public override bool CanConvert(Type objectType)
        {
            return typeof(GameConfiguration).IsAssignableFrom(objectType);
        }

        public override bool CanRead
        {
            get
            {
                if (_bypassRead)
                {
                    _bypassRead = false;
                    return false;
                }

                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                if (_bypassWrite)
                {
                    _bypassWrite = false;
                    return false;
                }

                return true;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var config = (GameConfiguration)value;

            if (_writeDepth > 0)
            {
                // Nested reference — write hex ID only.
                // ConfigurationLinker will resolve it back to the real object on load.
                writer.WriteValue(config.id.ToString());
                return;
            }

            // Root level — write the full object.
            // Set bypass so the inner serializer.Serialize call skips this converter
            // (otherwise we would recurse infinitely).
            _bypassWrite = true;
            _writeDepth++;
            try
            {
                serializer.Serialize(writer, value, typeof(GameConfiguration));
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[GameConfigurationJsonConverter] Error serializing {config.id}: {ex}");
            }
            finally
            {
                _writeDepth--;
                _bypassWrite = false; // safety: clear even if Serialize threw before CanWrite was checked
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // A plain string or integer token means this is an ID reference, not a full object.
            if (reader.TokenType == JsonToken.String || reader.TokenType == JsonToken.Integer)
            {
                var id = new ConfigurationId(reader.Value.ToString());
                return CreateStub(id, objectType);
            }

            // Full object — let the default deserializer handle fields.
            _bypassRead = true;
            return serializer.Deserialize(reader, objectType);
        }

        private static readonly Dictionary<Type, FieldInfo> _idFieldCache = new();

        private static GameConfiguration CreateStub(ConfigurationId id, Type objectType)
        {
            var stub = (GameConfiguration)Activator.CreateInstance(objectType);

            if (!_idFieldCache.TryGetValue(objectType, out var fieldInfo))
            {
                fieldInfo = typeof(GameConfiguration).GetField(
                    nameof(GameConfiguration.id),
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                _idFieldCache[objectType] = fieldInfo;
            }

            if (fieldInfo != null)
            {
                fieldInfo.SetValue(stub, id);
            }
            else
            {
                UnityEngine.Debug.LogError($"[GameConfigurationJsonConverter] Could not find 'id' field on {objectType.Name}");
            }

            return stub;
        }
    }
}
