using UnityEngine;


public static class HapticManager
{
    public static void Vibrate()
    {
        if (
            !GameSettings
                .HapticsEnabled
        )
        {
            return;
        }


#if UNITY_ANDROID || UNITY_IOS

        Handheld.Vibrate();

#endif
    }
}