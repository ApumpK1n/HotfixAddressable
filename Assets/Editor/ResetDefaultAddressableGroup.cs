using UnityEngine;
using System.Collections;
using UnityEditor;

public class ResetDefaultAddressableGroup
{

    [MenuItem("Tools/Reset Addressable Groups")]
    static void ResetGroups()
    {
        AddressableGroupSetter.ResetGroup("ConfigData", "Assets/ConfigData/Json", "t:TextAsset");
    }

    [InitializeOnLoadMethod]
    static void InitializeOnLoadMethod()
    {
        EditorApplication.projectChanged -= projectChanged;
        EditorApplication.projectChanged += projectChanged;
    }


    private static void projectChanged()
    {
        ResetGroups();
    }


}