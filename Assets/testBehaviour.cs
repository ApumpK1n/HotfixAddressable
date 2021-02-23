using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.Initialization;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class testBehaviour : MonoBehaviour
{
    private List<AsyncOperationHandle<GameObject>> handles = new List<AsyncOperationHandle<GameObject>>();
    private List<GameObject> instances = new List<GameObject>();

    private void Start()
    {
        var opInit = Addressables.InitializeAsync();
        opInit.Completed += handle =>
        {
            var resLocator = handle.Result;
            var resKeys = resLocator.Keys.ToList();
            Debug.LogFormat("Addressables.InitializeAsync Completed: {0}, {1}", resLocator.LocatorId, resKeys.Count);
            foreach (var key in resKeys)
            {
                IList<IResourceLocation> locations;
                var result = resLocator.Locate(key, typeof(Object), out locations);
                Debug.LogFormat("Res Locator Key: {0}, Count: {1}", key, result ? locations.Count : 0);
                if (result)
                {
                    for (int i = 0; i < locations.Count; i++)
                    {
                        Debug.LogFormat("[{0}] Resource Key: {1} Locator: {2}", i, key, locations[i]);
                    }
                }
            }
        };
        Debug.LogFormat("Addressable.Build__Path: " + Addressables.BuildPath);
        Debug.LogFormat("Addressable.RuntimePath: " + Addressables.RuntimePath);
    }

    private void OnGUI()
    {
        if (GUILayout.Button("CheckCatalogUpdate"))
        {
            Debug.Log("CheckCatalogUpdate");

            var opChecklist = Addressables.CheckForCatalogUpdates();
            opChecklist.Completed += handle =>
            {
                var checkList = handle.Result;
                Debug.LogFormat("CheckForCatalogUpdates: {0}, Status: {1}", checkList.Count, handle.Status);
                for (int i = 0; i < checkList.Count; i++)
                {
                    Debug.LogFormat("[{0}] Check Catalog: {1}", i, checkList[i]);
                }

                if (checkList.Count > 0)
                {
                    var opUpdate = Addressables.UpdateCatalogs(checkList);
                    opUpdate.Completed += operationHandle =>
                    {
                        Debug.LogFormat("UpdateCatalogs Status: {0}", operationHandle.Status);
                        if (operationHandle.Status == AsyncOperationStatus.Succeeded)
                        {
                            var locations = operationHandle.Result;
                            Debug.LogFormat("UpdateCatalogs: {0}", locations?.Count??0);
                            for (int i = 0; i < checkList.Count; i++)
                            {
                                Debug.LogFormat("[{0}] Resource Locator: {1}", i, checkList[i]);
                            }
                        }
                    };
                }
            };
        }

        if (GUILayout.Button("Download"))
        {
            var opInit = Addressables.InitializeAsync();
            opInit.Completed += opInitHandle =>
            {
                
                
                var resLocator = opInitHandle.Result;
                var opGetDownloadSize = Addressables.GetDownloadSizeAsync(resLocator.Keys);

                opGetDownloadSize.Completed += handle =>
                {
                    Debug.LogFormat("GetDownloadSizeAsync Status: {0} Size: {1} bytes", handle.GetDownloadStatus(), handle.Result);

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
                    

                    var allPrimaryKeys = allLocations.Distinct().ToList();
                    for (int i = 0; i < allPrimaryKeys.Count; i++)
                    {
                        Debug.LogFormat("[{0}]DownloadDependency PrimaryKey: {1}", i, allPrimaryKeys[i]);
                    }

                    var opDownload = Addressables.DownloadDependenciesAsync(allPrimaryKeys);

                    opDownload.Completed += operationHandle =>
                    {
                        Debug.LogFormat("DownloadDependenciesAsync: {0}", operationHandle.Result);
                    };
                };

            };
        }

        if (GUILayout.Button("ClearDependencyCache"))
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

        if (GUILayout.Button("Caching.ClearCache"))
        {
            var seconds = 1;
            var result = Caching.ClearCache(seconds);
            Debug.LogFormat("Caching.ClearCache: {0}", result);
        }

        if (GUILayout.Button("Load"))
        {
            Debug.Log("Load");

            var handle = Addressables.LoadAssetAsync<GameObject>("Assets/CanNotChangePost/A_1.prefab");
            handle.Completed += onLoadDone;
            
            handles.Add(handle);
        }
        if (GUILayout.Button("UnLoad"))
        {
            if (handles.Count > 0)
            {
                var handle = handles[handles.Count - 1];
            
                if (handle.IsValid())
                {
                    Debug.Log("UnLoad OK");

                    Addressables.Release(handle);
                }
                else
                {
                
                    Debug.Log("UnLoad Fail");

                }
            }


            if (instances.Count > 0)
            {
                Destroy(instances[instances.Count - 1]);
                instances.RemoveAt(instances.Count - 1);
            }
        }
    }

    void onLoadDone(AsyncOperationHandle<GameObject> h)
    {
        var obj = h.Result;
        var instance = Instantiate(obj);

        var text = instance.GetComponentInChildren<Text>();
        if (text)
        {
            text.text = instance.name + "\t" + text.text;

            ((RectTransform) text.transform).anchoredPosition += new Vector2(0, text.fontSize * instances.Count);
        }
        
        instances.Add(instance);
    }
}
