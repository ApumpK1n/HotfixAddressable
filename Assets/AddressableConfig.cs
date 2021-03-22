using UnityEngine;
public static class AddressableConfig
{

    public static string CdnPath = "http://lf3-ma42cdn-cn.ohayoo.cn/obj/light-game-cn/ma42";

    public static string CdnProjPath = "hotfixdemo/test";

    public static int UpdateTimes = 1;

    public static bool BackgroundDownload = false;

    public static string GetBuildTargetAtRuntime()
    {
#if UNITY_ANDROID
        return "Android";
#elif UNITY_IOS
        return "iOS";
#else
        return "StandaloneWindows";
#endif
    }

    public static string GetRuntimeRemoteLoadPath()
    {
        return CdnPath + "/" + CdnProjPath + "/" + GetBuildTargetAtRuntime() + "/" + Application.version;
    }
}
