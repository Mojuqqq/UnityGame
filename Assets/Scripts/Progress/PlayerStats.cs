using System;
using UnityEngine;


public static class PlayerStats
{
    // =====================================================
    // GENERAL STATS
    // =====================================================

    private const string TotalPiecesPlacedKey =
        "Puzzle_Stats_TotalPiecesPlaced";


    private const string TotalHintsUsedKey =
        "Puzzle_Stats_TotalHintsUsed";


    // =====================================================
    // DAILY STATS
    // =====================================================

    private const string DailyDateKey =
        "Puzzle_Stats_DailyDate";


    private const string DailyLevelsKey =
        "Puzzle_Stats_DailyLevels";


    private const string DailyHintsKey =
        "Puzzle_Stats_DailyHints";


    // =====================================================
    // TASK REWARD CLAIMS
    // =====================================================

    private const string AchievementClaimsKey =
        "Puzzle_Task_AchievementClaims";


    private const string DailyClaimsKey =
        "Puzzle_Task_DailyClaims";


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


        // Все награды Daily снова становятся
        // доступными в новом дне.
        PlayerPrefs.SetString(
            DailyClaimsKey,
            ""
        );


        PlayerPrefs.Save();
    }


    // =====================================================
    // PIECES
    // =====================================================

    public static void RecordPiecePlaced()
    {
        PlayerPrefs.SetInt(
            TotalPiecesPlacedKey,
            GetTotalPiecesPlaced() + 1
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


        PlayerPrefs.SetInt(
            DailyLevelsKey,
            GetDailyLevelsCompleted() + 1
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
    // TASK REWARD CLAIMS
    // =====================================================

    public static bool IsTaskRewardClaimed(
        string taskId,
        TaskCategory category
    )
    {
        if (category == TaskCategory.Daily)
        {
            EnsureDailyReset();
        }


        if (string.IsNullOrWhiteSpace(taskId))
        {
            return false;
        }


        string storageKey =
            GetClaimsStorageKey(
                category
            );


        string savedClaims =
            PlayerPrefs.GetString(
                storageKey,
                ""
            );


        string token =
            BuildClaimToken(
                taskId
            );


        return savedClaims.Contains(
            token
        );
    }


    public static void MarkTaskRewardClaimed(
        string taskId,
        TaskCategory category
    )
    {
        if (category == TaskCategory.Daily)
        {
            EnsureDailyReset();
        }


        if (string.IsNullOrWhiteSpace(taskId))
        {
            return;
        }


        string storageKey =
            GetClaimsStorageKey(
                category
            );


        string savedClaims =
            PlayerPrefs.GetString(
                storageKey,
                ""
            );


        string token =
            BuildClaimToken(
                taskId
            );


        if (savedClaims.Contains(token))
        {
            return;
        }


        savedClaims +=
            token;


        PlayerPrefs.SetString(
            storageKey,
            savedClaims
        );


        PlayerPrefs.Save();
    }


    private static string GetClaimsStorageKey(
        TaskCategory category
    )
    {
        switch (category)
        {
            case TaskCategory.Daily:

                return
                    DailyClaimsKey;


            case TaskCategory.Achievement:

                return
                    AchievementClaimsKey;
        }


        return
            AchievementClaimsKey;
    }


    private static string BuildClaimToken(
        string taskId
    )
    {
        return
            $"|{taskId.Trim()}|";
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


        PlayerPrefs.DeleteKey(
            AchievementClaimsKey
        );


        PlayerPrefs.DeleteKey(
            DailyClaimsKey
        );


        // Старые ключи от предыдущей
        // версии системы наград.
        PlayerPrefs.DeleteKey(
            "Puzzle_Daily_Reward_1_Claimed"
        );

        PlayerPrefs.DeleteKey(
            "Puzzle_Daily_Reward_2_Claimed"
        );

        PlayerPrefs.DeleteKey(
            "Puzzle_Daily_Reward_3_Claimed"
        );


        PlayerPrefs.DeleteKey(
            "Puzzle_Achievement_Reward_1_Claimed"
        );

        PlayerPrefs.DeleteKey(
            "Puzzle_Achievement_Reward_2_Claimed"
        );

        PlayerPrefs.DeleteKey(
            "Puzzle_Achievement_Reward_3_Claimed"
        );


        PlayerPrefs.Save();
    }
}