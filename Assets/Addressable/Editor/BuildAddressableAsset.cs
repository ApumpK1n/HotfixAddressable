using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets;
using System.IO;

public class BuildAddressableAsset
{
    [MenuItem("Window/Asset Management/BuildAddressableAsset/BuildContent")]
    public static void BuildContent()
    {
        CreateVersionBuild.CreateVersion();
        SetDataBuilder();
        AddressableAssetSettings.CleanPlayerContent(AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilder);
        AddressableAssetSettings.BuildPlayerContent();
    }

    [MenuItem("Window/Asset Management/BuildAddressableAsset/BuildUpdate")]
    public static void BuildUpdate()
    {
        CreateVersionBuild.CreateVersion();
        SetDataBuilder();
        string path = ContentUpdateScript.GetContentStateDataPath(false);
        ContentUpdateScript.BuildContentUpdate(AddressableAssetSettingsDefaultObject.Settings, path);
    }

    public static void ShellBuild(string profile = "Default")
    {
        SetDataBuilder();
        var aaSettings = AddressableAssetSettingsDefaultObject.Settings;

        if (aaSettings != null && aaSettings.BuildRemoteCatalog)
        {
            var id = aaSettings.profileSettings.GetProfileId(profile);
            aaSettings.activeProfileId = id;
            string path = ContentUpdateScript.GetContentStateDataPath(false);
            if (File.Exists(path))
            {
                ContentUpdateScript.BuildContentUpdate(AddressableAssetSettingsDefaultObject.Settings, path);
            }
            else
            {
                AddressableAssetSettings.BuildPlayerContent();
            }
        }

    }

    public static void SetDataBuilder(string name = "Use Existing Build (requires built groups)")
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        for (int i = 0; i < settings.DataBuilders.Count; i++)
        {
            var dataBuilder = settings.GetDataBuilder(i);
            if (name == dataBuilder.Name)
            {
                AddressableAssetSettingsDefaultObject.Settings.ActivePlayModeDataBuilderIndex = i;
                return;
            }
        }
    }
}

