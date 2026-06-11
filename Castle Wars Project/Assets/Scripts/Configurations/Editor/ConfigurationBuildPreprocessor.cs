#if UNITY_EDITOR
using UnityEditor.Build.Reporting;
using UnityEditor.Build;
using UnityEditor;
using UnityEngine;
using System.Linq;
using Configurations;

public sealed class ConfigurationBuildPreprocessor : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        Debug.Log("Rebuild config file");

        SaveFinalAssetsForAllConfigs();
    }

    [MenuItem("Tools/Rebuild all configurations")]
    private static void SaveFinalAssetsForAllConfigs()
    {
        var configs = FindAllScriptableObjects<ConfigurationContainer>();
        foreach (var config in configs)
        {
            config.LoadSources();
            config.SaveBuildIn();
        }

        AssetDatabase.Refresh();
    }

    public static T[] FindAllScriptableObjects<T>() where T : ScriptableObject
    {
        string filter = $"t:{typeof(T).Name}"; // Search by type
        string[] guids = AssetDatabase.FindAssets(filter); // find all GUID
        return guids
            .Select(AssetDatabase.GUIDToAssetPath) // Exchange GUID to path
            .Select(AssetDatabase.LoadAssetAtPath<T>) // Load object
            .Where(asset => asset != null) // clear nulls
            .ToArray();
    }
}
#endif