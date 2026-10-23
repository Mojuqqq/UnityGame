using UnityEngine;


public static class PlayerEntitlements
{
    private const string AdsRemovedKey =
        "Puzzle_Ads_Removed";


    public static bool AreAdsRemoved()
    {
        return
            PlayerPrefs.GetInt(
                AdsRemovedKey,
                0
            ) == 1;
    }


    public static void SetAdsRemoved(
        bool value
    )
    {
        PlayerPrefs.SetInt(
            AdsRemovedKey,
            value ? 1 : 0
        );


        PlayerPrefs.Save();
    }


    public static void ResetForTesting()
    {
        PlayerPrefs.DeleteKey(
            AdsRemovedKey
        );


        PlayerPrefs.Save();
    }
}