using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using System;

public class AddressableGroupSetter : ScriptableObject
{
    public static AddressableAssetSettings Settings
    {
        get { return AddressableAssetSettingsDefaultObject.Settings; }
    }

    /// <summary>
    /// 重置某分组
    /// </summary>
    /// <param name="groupName">组名</param>
    /// <param name="assetFolder">资源目录</param>
    /// <param name="filter">过滤器：
    public static void ResetGroup(string groupName, string assetFolder, string filter)
    {
        string[] assets = GetAssets(assetFolder, filter);
        AddressableAssetGroup group = CreateGroup(groupName);
        foreach (var assetPath in assets)
        {
            AddAssetEntry(group, assetPath, assetPath);
        }

        Debug.Log($"Reset group finished, group: {groupName}, asset folder: {assetFolder}, filter: {filter}, count: {assets.Length}");
    }

    // 创建分组
    public static AddressableAssetGroup CreateGroup(string groupName)
    {
        AddressableAssetGroup group = Settings.FindGroup(groupName);
        if (group == null)
            group = Settings.CreateGroup(groupName, false, false, false, Settings.DefaultGroup.Schemas);

        Settings.SetDirty(AddressableAssetSettings.ModificationEvent.GroupAdded, group, true);
        Settings.AddLabel(groupName, false);
        return group;
    }

    // 给某分组添加资源
    public static AddressableAssetEntry AddAssetEntry(AddressableAssetGroup group, string assetPath, string address)
    {
        string guid = AssetDatabase.AssetPathToGUID(assetPath);

        AddressableAssetEntry entry = group.entries.FirstOrDefault(e => e.guid == guid);
        if (entry == null)
        {
            entry = Settings.CreateOrMoveEntry(guid, group, false, false);
        }

        entry.address = address;
        entry.SetLabel(group.Name, true, false, false);

        Settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
        return entry;
    }

    /// <summary>
    /// 获取指定目录的资源
    /// </summary>
    /// <param name="filter">过滤器：
    public static string[] GetAssets(string folder, string filter)
    {
        if (string.IsNullOrEmpty(folder))
            throw new ArgumentException("folder");
        if (string.IsNullOrEmpty(filter))
            throw new ArgumentException("filter");

        folder = folder.TrimEnd('/').TrimEnd('\\');

        string[] guids = AssetDatabase.FindAssets(filter, new string[] { folder });
        string[] paths = new string[guids.Length];
        for (int i = 0; i < guids.Length; i++)
            paths[i] = AssetDatabase.GUIDToAssetPath(guids[i]);
        return paths;
    }
}