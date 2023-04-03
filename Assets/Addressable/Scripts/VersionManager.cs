using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.IO;
using System;

public class VersionManager : MonoBehaviour
{
    private string versionRequest = "";
    private string versionName = "version";
    public Text m_showVersion;

    private string version;

    private string savePath;
        // Use this for initialization
    void Start()
    {
        savePath = Application.persistentDataPath + "/addressable/version";
        string dirPath = Application.persistentDataPath + "/addressable";
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        if (!File.Exists(savePath))
        {
            File.Create(savePath);
        }

        versionRequest = AddressableConfig.GetRuntimeRemoteLoadPath();
        StartCoroutine(Download());
    }

    private IEnumerator Download()
    {
        string url = versionRequest + "/" + versionName;
        Debug.Log("Download:" + url);
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        byte[] results = request.downloadHandler.data;

        using (StreamWriter writer = new StreamWriter(savePath, false))
        {
            foreach (byte by in results)
            {
                char result = Convert.ToChar(by);
                writer.Write(result);
            }
        }
        ReadVersion();
    }

    public void ReadVersion()
    {
        using (StreamReader reader = new StreamReader(savePath))
        {
            string result = reader.ReadToEnd();
            version = result;
        }
        ShowVersion();
    }

    // Update is called once per frame
    private void ShowVersion()
    {
        m_showVersion.text = versionName + ":" + version;
    }
}