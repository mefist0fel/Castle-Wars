using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using Configurations.Model;
using System;
using Configurations.Model.Serialization;
using Configurations.Model.Id;
using System.Linq;
using System.Reflection;

namespace Configurations
{
    [CreateAssetMenu(fileName = "configuration", menuName = "Config/Container")]
    public sealed class ConfigurationContainer : ScriptableObject
    {
        [Header("Build in asset")]
        [SerializeField]
        private LazyLoadReference<TextAsset> buildInConfiguration;
        [SerializeField]
        private string buildInConfigurationFilePath = "Assets/Config/config.json";

        [Header("Editor time sources")]
        [SerializeField]
        private string configurationsSourcesPath = "Configs/";
        [SerializeField]
        private bool loadSourcesInEditor = true;

        public Configuration Configuration => GetConfiguration();

        private Configuration cachedConfiguration = null;

        public GameConfiguration Get(ConfigurationId id) => Configuration.Get(id);
        public GameConfiguration Get<T>(ConfigurationId id) where T : GameConfiguration => Configuration.Get<T>(id);

        private Configuration GetConfiguration()
        {
            cachedConfiguration ??= LoadConfiguration();
            return cachedConfiguration;
        }

        public Configuration ReloadConfiguration()
        {
            cachedConfiguration = LoadConfiguration();
            return cachedConfiguration;
        }

        private Configuration LoadConfiguration()
        {
#if UNITY_EDITOR
            if (loadSourcesInEditor)
            {
                return LoadFromSources();
            }
#endif
            return LoadBuildInJson();
        }

        private Configuration LoadBuildInJson()
        {
            try
            {
                var configs = JsonConvert.DeserializeObject<Dictionary<ConfigurationId, GameConfiguration>>(buildInConfiguration.asset.text, SerializerSettings);
                ConfigurationLinker.LinkConfigurationReferences(configs);
                return new Configuration(configs);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            return new();
        }

        public void LoadSources()
        {
            if (cachedConfiguration != null)
                return;

            cachedConfiguration = LoadFromSources();
        }

        private readonly Dictionary<ConfigurationId, string> loadedSourceFiles = new();
        private readonly Dictionary<ConfigurationId, string> configJsons = new();
        private Configuration LoadFromSources()
        {
            var time = System.Diagnostics.Stopwatch.StartNew();
            loadedSourceFiles.Clear();
            var sourceFiles = Directory.GetFiles(Path.Combine(Application.dataPath, configurationsSourcesPath), "*.json");
            Dictionary<ConfigurationId, GameConfiguration> configs = new(sourceFiles.Length);
            foreach (var sourceFile in sourceFiles)
            {
                try
                {
                    var json = File.ReadAllText(sourceFile);
                    var config = JsonConvert.DeserializeObject<GameConfiguration>(json, SerializerSettings);
                    configs.Add(config.id, config);
                    configJsons[config.id] = json;
                    loadedSourceFiles[config.id] = sourceFile;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Can't load config file {sourceFile} with {ex.Message}");
                }
            }
            ConfigurationLinker.LinkConfigurationReferences(configs);
            Debug.Log($"load {sourceFiles.Length} files in {time.Elapsed}:\n{string.Join("\n", sourceFiles)}");
            return new(configs);
        }

        public void SaveSources()
        {
            if (cachedConfiguration == null || cachedConfiguration.Configurations == null)
                return;

            CheckFilesRenaming();
            foreach (var configuration in Configuration.Configurations.Values)
            {
                var fileName = FixFileName(configuration.name) + ".json";
                if (string.IsNullOrEmpty(fileName))
                {
                    Debug.LogError($"can't save file name for config {configuration.id} - name is empty");
                    continue;
                }
                var serializedJson = JsonConvert.SerializeObject(configuration, typeof(GameConfiguration), Formatting.Indented, SerializerSettings);
                if (string.IsNullOrEmpty(serializedJson) || configJsons.TryGetValue(configuration.id, out var oldJson) && (oldJson == serializedJson))
                {
                    continue;
                }
                var path = Path.Combine(Application.dataPath, configurationsSourcesPath, fileName);
                File.WriteAllText(path, serializedJson);
                configJsons[configuration.id] = serializedJson;
                Debug.Log($"Saved source {configuration.name} [{configuration.id}]");
            }
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
            Debug.Log($"Saved sources {cachedConfiguration.Configurations.Count}");
        }

        private void CheckFilesRenaming()
        {
            foreach (var configuration in Configuration.Configurations.Values)
            {
                var fileName = FixFileName(configuration.name) + ".json";
                var path = Path.Combine(Application.dataPath, configurationsSourcesPath, fileName);
                // Rename if needed
                if (loadedSourceFiles.TryGetValue(configuration.id, out var oldPath))
                {
                    if (path != oldPath)
                    {
                        Debug.Log($"Moved config file from '{oldPath}' to '{path}' (and meta from {oldPath}.meta to {path}.meta");
                        MoveSafe(oldPath, path);
                        MoveSafe($"{oldPath}.meta", $"{path}.meta");
                    }
                    // file is processed
                    loadedSourceFiles.Remove(configuration.id);
                }
            }
            // remove not used files (configs were deleted)
            foreach (var sourceFile in loadedSourceFiles.Values)
            {
                DeleteSafe(sourceFile);
                DeleteSafe(sourceFile.Replace(".json", ".meta"));
                Debug.Log($"Deleted not used config file from {sourceFile}");
            }
            loadedSourceFiles.Clear();
            foreach (var configuration in Configuration.Configurations.Values)
            {
                var fileName = FixFileName(configuration.name) + ".json";
                var path = Path.Combine(Application.dataPath, configurationsSourcesPath, fileName);
                loadedSourceFiles.Add(configuration.id, path);
            }
        }

        private void MoveSafe(string source, string destination)
        {
            if (File.Exists(source))
            {
                File.Move(source, destination);
            }
        }

        private void DeleteSafe(string sourceFile)
        {
            if (File.Exists(sourceFile))
            {
                File.Delete(sourceFile);
            }
        }

        public static string FixFileName(string name, char replacement = '_', int maxLength = 255)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            string cleanName = string.Concat(name.Select(c => invalidChars.Contains(c) ? replacement : c));
            return cleanName.Length > maxLength ? cleanName.Substring(0, maxLength) : cleanName;
        }

        internal static readonly JsonSerializerSettings SerializerSettings = BuildSerializerSettings();

        private static JsonSerializerSettings BuildSerializerSettings()
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                Converters = {
                    new ConfigurationIdJsonConverter(),
                    new GameConfigurationJsonConverter(),
                    new SpriteIdJsonConverter(),
                    new AssetIdJsonConverter()
                },
                SerializationBinder = new AllAssembliesSerializationBinder(),
                ContractResolver = new SerializationContractResolver(),
            };

            var converterBaseType = typeof(JsonConverter);
            var attrType = typeof(ConfigurationConverterAttribute);
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch
                {
                    continue;
                }

                foreach (var type in types)
                {
                    if (type.IsAbstract || !converterBaseType.IsAssignableFrom(type))
                        continue;
                    if (type.GetCustomAttribute(attrType) == null)
                        continue;

                    try
                    {
                        settings.Converters.Add((JsonConverter)Activator.CreateInstance(type));
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[ConfigurationContainer] Could not instantiate converter {type.FullName}: {ex.Message}");
                    }
                }
            }

            return settings;
        }

#if UNITY_EDITOR
        [ContextMenu("Save build-in Json")]
        public void SaveBuildIn()
        {
            string json = JsonConvert.SerializeObject(Configuration.Configurations, Formatting.Indented, SerializerSettings);

            if (buildInConfiguration.asset == null)
            {
                buildInConfiguration = CreateNewAsset(buildInConfigurationFilePath, json);
            }
            else
            {
                string assetPath = UnityEditor.AssetDatabase.GetAssetPath(buildInConfiguration.asset);
                if (string.IsNullOrEmpty(assetPath))
                {
                    Debug.LogError("Can't find asset path!");
                    return;
                }
                // Rewrite config file
                File.WriteAllText(assetPath, json);
                // Update asset in Unity
                UnityEditor.AssetDatabase.ImportAsset(assetPath);
            }

            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
        }

        private LazyLoadReference<TextAsset> CreateNewAsset(string path, string json)
        {
            if (!File.Exists(path))
            {
                File.WriteAllText(path, json);
                UnityEditor.AssetDatabase.ImportAsset(path);
            }

            // Loading TextAsset and attach it to ScriptableObject
            return new LazyLoadReference<TextAsset>(UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(path));
        }
#endif
    }
}