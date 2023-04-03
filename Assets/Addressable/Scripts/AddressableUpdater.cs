using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AddressableUpdater : MonoBehaviour
{
    [SerializeField] private Text statusText;
    public const float CHECKTIMEMAX = 10;
    public string UpdateDoneLoadScene;

    private bool checkingUpdate;
    private bool needUpdate;

    private float checkUpdateTime = 0;

    private List<string> needUpdateCatalogs;
    private AsyncOperationHandle<List<IResourceLocator>> updateHandle;
    private List<object> needUpdateKeys = new List<object>();

    private void Start()
    {
        DontDestroyOnLoad(this);
        StartCheckUpdate();
    }

    public void StartCheckUpdate()
    {
        statusText.text = "正在检测资源更新...";
        StartCoroutine(checkUpdate());
    }

    public void ClearDependencyCache()
    {
        var opInit = Addressables.InitializeAsync();
        opInit.Completed += opInitHandle =>
        {
            var resLocator = opInitHandle.Result;

            var allLocations = new List<IResourceLocation>();
            foreach (var key in resLocator.Keys)
            {
                IList<IResourceLocation> locations;
                if (resLocator.Locate(key, typeof(object), out locations))
                {
                    foreach (var loc in locations)
                    {
                        if (loc.HasDependencies)
                            allLocations.AddRange(loc.Dependencies);
                    }
                }
            }

            Debug.LogFormat("ClearDependencyCacheAsync: {0}", allLocations.Count);

            var opClear = Addressables.ClearDependencyCacheAsync(allLocations, false);
            opClear.Completed += handle =>
            {
                Debug.LogFormat("ClearDependencyCacheAsync Completed: {0}", handle.Result);
                Addressables.Release(handle);
            };
        };
    }

    public void StartUpdate()
    {
        if (needUpdate)
        {
            StartCoroutine(UpdateCatalog());
        }
    }

    public void StartDownload()
    {
        StartCoroutine(Download());
        if (AddressableConfig.BackgroundDownload)
        {
            Skip();
        }
    }


    IEnumerator checkUpdate()
    {
        checkingUpdate = true;
        //初始化Addressable
        var init = Addressables.InitializeAsync();
        yield return init;

        var start = DateTime.Now;
        //检测是否有Catalog更新
        AsyncOperationHandle<List<string>> handle = Addressables.CheckForCatalogUpdates(false);
        //检查结束，验证结果
        checkingUpdate = false;
        Debug.Log(string.Format("CheckIfNeededUpdate use {0}ms", (DateTime.Now - start).Milliseconds));
        yield return handle;
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            List<string> catalogs = handle.Result;
            if (catalogs != null && catalogs.Count > 0)
            {
                needUpdate = true;
                needUpdateCatalogs = catalogs;
            }
        }

        if (needUpdate)
        {
            statusText.text = "有资源需要更新";

            StartUpdate();
        }
        else
        {
            Skip();
        }

        Addressables.Release(handle);
    }

    IEnumerator UpdateCatalog()
    {
        var start = DateTime.Now;

        updateHandle = Addressables.UpdateCatalogs(needUpdateCatalogs, false);
        yield return updateHandle;
        Debug.Log(string.Format("UpdateCatalogFinish use {0}ms", (DateTime.Now - start).Milliseconds));

        foreach (var item in updateHandle.Result)
        {
            Debug.Log("catalog result " + item.LocatorId);
            foreach (var key in item.Keys)
            {
                Debug.Log("catalog key " + key);
            }
            needUpdateKeys.AddRange(item.Keys);
        }

        if (needUpdateKeys.Count > 0)
        {
            StartDownload();
        }
        else
        {
            Skip();
        }

        Addressables.Release(updateHandle);
    }

    IEnumerator Download()
    {
        var downloadsize = Addressables.GetDownloadSizeAsync(needUpdateKeys);
        yield return downloadsize;

        long totalDownloadSize = downloadsize.Result;
        Debug.Log("start download size :" + totalDownloadSize);

        if (totalDownloadSize > 0)
        {
            statusText.text = "开始下载资源";
            var downloadHandle = Addressables.DownloadDependenciesAsync(needUpdateKeys, Addressables.MergeMode.Union);
            while (!downloadHandle.IsDone)
            {
                float percent = downloadHandle.PercentComplete;
                statusText.text = $"已经下载：{(int)(totalDownloadSize * percent)}/{totalDownloadSize}";
                yield return null;
            }
            Debug.Log("download result type " + downloadHandle.Result.GetType());
            statusText.text = "下载完成";
            foreach (var item in downloadHandle.Result as List<UnityEngine.ResourceManagement.ResourceProviders.IAssetBundleResource>)
            {
                var ab = item.GetAssetBundle();
                Debug.Log("ab name " + ab.name);
                foreach (var name in ab.GetAllAssetNames())
                {
                    Debug.Log("asset name " + name);
                }
            }
            Addressables.Release(downloadHandle);
        }
        Addressables.Release(downloadsize);

        Skip();
    }

    public void Skip()
    {
        Debug.Log("结束");
        statusText.gameObject.SetActive(false);
        SceneManager.LoadSceneAsync(UpdateDoneLoadScene, LoadSceneMode.Additive).completed += operation =>
        {
            SceneManager.UnloadSceneAsync("AddressableUpdating");
        };
    }

    private void Update()
    {
        if (checkingUpdate)
        {
            checkUpdateTime += Time.deltaTime;
            if (checkUpdateTime > CHECKTIMEMAX)
            {
                checkingUpdate = false;
                StopAllCoroutines();
                Skip();
                Debug.Log(string.Format("Connect Timed Out"));
            }
        }
    }
}