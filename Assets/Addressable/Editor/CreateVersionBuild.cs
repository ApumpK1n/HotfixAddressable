using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;


public class CreateVersionBuild : EditorWindow
{
    [MenuItem("Window/Asset Management/Create Version")]
    public static void CreateVersion()
    {
        string fileName = "version";

        string playerVersion = AddressableAssetSettingsDefaultObject.Settings.PlayerBuildVersion;
        
        string profileId = AddressableAssetSettingsDefaultObject.Settings.activeProfileId;

        AddressableAssetProfileSettings profileSettings = AddressableAssetSettingsDefaultObject.Settings.profileSettings;

        string updateTimes = AddressableConfig.UpdateTimes.ToString();

        string remoteBuildPath = profileSettings.EvaluateString(profileId, profileSettings.GetValueByName(profileId, "RemoteBuildPath"));

        string filePath = remoteBuildPath + "/" + fileName;
        if (!Directory.Exists(remoteBuildPath))
        {
            Directory.CreateDirectory(remoteBuildPath); 
        }

        using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
        {
        }

        using (StreamWriter writer = new StreamWriter(filePath, false))
        {
            writer.WriteLine(playerVersion + "." + updateTimes);
        }
    }
}
