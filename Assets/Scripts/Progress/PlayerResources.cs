using UnityEngine;


public enum HintType
{
    RevealPiece,
    RevealEmptyCells
}


public static class PlayerResources
{
    private const string CoinsKey =
        "Puzzle_Player_Coins";


    private const string RevealPieceHintsKey =
        "Puzzle_Hints_RevealPiece";


    private const string EmptyCellsHintsKey =
        "Puzzle_Hints_EmptyCells";


    // Пока тестовые стартовые значения.
    private const int DefaultCoins =
        0;

    private const int DefaultRevealPieceHints =
        2;

    private const int DefaultEmptyCellsHints =
        2;


    // =====================================================
    // COINS
    // =====================================================

    public static int GetCoins()
    {
        return PlayerPrefs.GetInt(
            CoinsKey,
            DefaultCoins
        );
    }


    public static void AddCoins(
        int amount
    )
    {
        if (amount <= 0)
        {
            return;
        }


        PlayerPrefs.SetInt(
            CoinsKey,
            GetCoins() + amount
        );


        PlayerPrefs.Save();
    }


    public static bool TrySpendCoins(
        int amount
    )
    {
        if (amount <= 0)
        {
            return true;
        }


        int current =
            GetCoins();


        if (current < amount)
        {
            return false;
        }


        PlayerPrefs.SetInt(
            CoinsKey,
            current - amount
        );


        PlayerPrefs.Save();


        return true;
    }


    // =====================================================
    // HINTS
    // =====================================================

    public static int GetHintCount(
        HintType type
    )
    {
        switch (type)
        {
            case HintType.RevealPiece:

                return PlayerPrefs.GetInt(
                    RevealPieceHintsKey,
                    DefaultRevealPieceHints
                );


            case HintType.RevealEmptyCells:

                return PlayerPrefs.GetInt(
                    EmptyCellsHintsKey,
                    DefaultEmptyCellsHints
                );
        }


        return 0;
    }


    public static void AddHints(
        HintType type,
        int amount
    )
    {
        if (amount <= 0)
        {
            return;
        }


        string key =
            GetHintKey(
                type
            );


        PlayerPrefs.SetInt(
            key,
            GetHintCount(type) + amount
        );


        PlayerPrefs.Save();
    }


    public static bool TrySpendHint(
        HintType type
    )
    {
        int current =
            GetHintCount(
                type
            );


        if (current <= 0)
        {
            return false;
        }


        PlayerPrefs.SetInt(
            GetHintKey(type),
            current - 1
        );


        PlayerPrefs.Save();


        return true;
    }


    private static string GetHintKey(
        HintType type
    )
    {
        switch (type)
        {
            case HintType.RevealPiece:

                return
                    RevealPieceHintsKey;


            case HintType.RevealEmptyCells:

                return
                    EmptyCellsHintsKey;
        }


        return "";
    }


    // =====================================================
    // TEST RESET
    // =====================================================

    public static void ResetForTesting()
    {
        PlayerPrefs.DeleteKey(
            CoinsKey
        );


        PlayerPrefs.DeleteKey(
            RevealPieceHintsKey
        );


        PlayerPrefs.DeleteKey(
            EmptyCellsHintsKey
        );


        // Старый ресурс больше не используется.
        PlayerPrefs.DeleteKey(
            "Puzzle_Player_Hints"
        );


        PlayerPrefs.Save();
    }
}