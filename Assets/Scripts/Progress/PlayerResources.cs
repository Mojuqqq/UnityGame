using UnityEngine;

public static class PlayerResources
{
    private const string HintsKey =
        "Puzzle_Player_Hints";


    private const int DefaultHints =
        5;


    // =====================================================
    // HINTS
    // =====================================================

    public static int GetHints()
    {
        return
            PlayerPrefs.GetInt(
                HintsKey,
                DefaultHints
            );
    }


    public static void AddHints(
        int amount
    )
    {
        if (amount <= 0)
        {
            return;
        }


        int current =
            GetHints();


        PlayerPrefs.SetInt(
            HintsKey,
            current + amount
        );


        PlayerPrefs.Save();
    }


    public static bool TrySpendHint()
    {
        int current =
            GetHints();


        if (current <= 0)
        {
            return false;
        }


        PlayerPrefs.SetInt(
            HintsKey,
            current - 1
        );


        PlayerPrefs.Save();


        return true;
    }


    // =====================================================
    // TEST
    // =====================================================

    public static void ResetForTesting()
    {
        PlayerPrefs.DeleteKey(
            HintsKey
        );


        PlayerPrefs.Save();
    }
}