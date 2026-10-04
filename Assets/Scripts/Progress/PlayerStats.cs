using System;
using UnityEngine;


public static class PlayerStats
{
    private const string TotalPiecesPlacedKey =
        "Puzzle_Stats_TotalPiecesPlaced";


    private const string TotalHintsUsedKey =
        "Puzzle_Stats_TotalHintsUsed";


    private const string DailyDateKey =
        "Puzzle_Stats_DailyDate";


    private const string DailyLevelsKey =
        "Puzzle_Stats_DailyLevels";


    private const string DailyHintsKey =
        "Puzzle_Stats_DailyHints";


    // =====================================================
    // DAILY RESET
    // =====================================================

    public static void EnsureDailyReset()
    {
        string today =
            DateTime.Now.ToString(
                "yyyyMMdd"
            );


        string savedDate =
            PlayerPrefs.GetString(
                DailyDateKey,
                ""
            );


        if (savedDate == today)
        {
            return;
        }


        PlayerPrefs.SetString(
            DailyDateKey,
            today
        );


        PlayerPrefs.SetInt(
            DailyLevelsKey,
            0
        );


        PlayerPrefs.SetInt(
            DailyHintsKey,
            0
        );


        PlayerPrefs.Save();
    }


    // =====================================================
    // PIECES
    // =====================================================

    public static void RecordPiecePlaced()
    {
        int current =
            GetTotalPiecesPlaced();


        PlayerPrefs.SetInt(
            TotalPiecesPlacedKey,
            current + 1
        );


        PlayerPrefs.Save();
    }


    public static int GetTotalPiecesPlaced()
    {
        return PlayerPrefs.GetInt(
            TotalPiecesPlacedKey,
            0
        );
    }


    // =====================================================
    // LEVELS
    // =====================================================

    public static void RecordLevelCompletedToday()
    {
        EnsureDailyReset();


        int current =
            GetDailyLevelsCompleted();


        PlayerPrefs.SetInt(
            DailyLevelsKey,
            current + 1
        );


        PlayerPrefs.Save();
    }


    public static int GetDailyLevelsCompleted()
    {
        EnsureDailyReset();


        return PlayerPrefs.GetInt(
            DailyLevelsKey,
            0
        );
    }


    // =====================================================
    // HINTS
    // =====================================================

    public static void RecordHintUsed()
    {
        EnsureDailyReset();


        PlayerPrefs.SetInt(
            TotalHintsUsedKey,
            GetTotalHintsUsed() + 1
        );


        PlayerPrefs.SetInt(
            DailyHintsKey,
            GetDailyHintsUsed() + 1
        );


        PlayerPrefs.Save();
    }


    public static int GetTotalHintsUsed()
    {
        return PlayerPrefs.GetInt(
            TotalHintsUsedKey,
            0
        );
    }


    public static int GetDailyHintsUsed()
    {
        EnsureDailyReset();


        return PlayerPrefs.GetInt(
            DailyHintsKey,
            0
        );
    }


    // =====================================================
    // DAILY TASK STATE
    // =====================================================

    public static int GetCompletedDailyTaskCount()
    {
        int completed =
            0;


        int levels =
            GetDailyLevelsCompleted();


        int hints =
            GetDailyHintsUsed();


        if (levels >= 1)
        {
            completed++;
        }


        if (levels >= 3)
        {
            completed++;
        }


        if (hints >= 1)
        {
            completed++;
        }


        return completed;
    }


    // =====================================================
    // RESET
    // =====================================================

    public static void ResetForTesting()
    {
        PlayerPrefs.DeleteKey(
            TotalPiecesPlacedKey
        );


        PlayerPrefs.DeleteKey(
            TotalHintsUsedKey
        );


        PlayerPrefs.DeleteKey(
            DailyDateKey
        );


        PlayerPrefs.DeleteKey(
            DailyLevelsKey
        );


        PlayerPrefs.DeleteKey(
            DailyHintsKey
        );


        PlayerPrefs.Save();
    }
}