using UnityEngine;

public static class PortraitOrientationBootstrap
{
    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.BeforeSceneLoad
    )]
    private static void ApplyPortraitOrientation()
    {
#if UNITY_ANDROID || UNITY_IOS

        Screen.autorotateToPortrait =
            true;

        Screen.autorotateToPortraitUpsideDown =
            false;

        Screen.autorotateToLandscapeLeft =
            false;

        Screen.autorotateToLandscapeRight =
            false;

        Screen.orientation =
            ScreenOrientation.Portrait;

#endif
    }
}