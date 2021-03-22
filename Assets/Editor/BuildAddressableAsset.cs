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
        AddressableAssetSettings.BuildPlayerContent();
    }

    [MenuItem("Window/Asset Management/BuildAddressableAsset/BuildUpdate")]
    public static void BuildUpdate()
    {
        CreateVersionBuild.CreateVersion();
        string path = ContentUpdateScript.GetContentStateDataPath(false);
        ContentUpdateScript.BuildContentUpdate(AddressableAssetSettingsDefaultObject.Settings, path);
    }

    public static void ShellBuild(string profile = "Default")
    {
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
}

