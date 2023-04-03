using UnityEngine;
public static class AddressableConfig
{

    // cdn路径
    public static string CdnPath = "http://lf3-ma42cdn-cn.ohayoo.cn/obj/light-game-cn/ma42";

    //项目路径
    public static string CdnProjPath = "hotfixdemo/test";

    // 此版本热更次数
    public static int UpdateTimes = 1;

    // 是否后台下载
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
