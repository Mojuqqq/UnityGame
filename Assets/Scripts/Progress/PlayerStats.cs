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
    // DAILY REWARD CLAIMS
    // =====================================================

    private const string DailyReward1ClaimedKey =
        "Puzzle_Daily_Reward_1_Claimed";

    private const string DailyReward2ClaimedKey =
        "Puzzle_Daily_Reward_2_Claimed";

    private const string DailyReward3ClaimedKey =
        "Puzzle_Daily_Reward_3_Claimed";


    // =====================================================
    // ACHIEVEMENT REWARD CLAIMS
    // =====================================================

    private const string AchievementReward1ClaimedKey =
        "Puzzle_Achievement_Reward_1_Claimed";

    private const string AchievementReward2ClaimedKey =
        "Puzzle_Achievement_Reward_2_Claimed";

    private const string AchievementReward3ClaimedKey =
        "Puzzle_Achievement_Reward_3_Claimed";


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


        // Новый день:
        // дневные награды снова можно получить.
        PlayerPrefs.SetInt(
            DailyReward1ClaimedKey,
            0
        );

        PlayerPrefs.SetInt(
            DailyReward2ClaimedKey,
            0
        );

        PlayerPrefs.SetInt(
            DailyReward3ClaimedKey,
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
    // DAILY REWARD CLAIM STATE
    // =====================================================

    public static bool IsDailyRewardClaimed(
        int index
    )
    {
        EnsureDailyReset();


        return PlayerPrefs.GetInt(
            GetDailyRewardKey(index),
            0
        ) == 1;
    }


    public static void MarkDailyRewardClaimed(
        int index
    )
    {
        EnsureDailyReset();


        string key =
            GetDailyRewardKey(
                index
            );


        if (string.IsNullOrEmpty(key))
        {
            return;
        }


        PlayerPrefs.SetInt(
            key,
            1
        );


        PlayerPrefs.Save();
    }


    private static string GetDailyRewardKey(
        int index
    )
    {
        switch (index)
        {
            case 0:
                return DailyReward1ClaimedKey;

            case 1:
                return DailyReward2ClaimedKey;

            case 2:
                return DailyReward3ClaimedKey;
        }


        return "";
    }


    // =====================================================
    // ACHIEVEMENT REWARD CLAIM STATE
    // =====================================================

    public static bool IsAchievementRewardClaimed(
        int index
    )
    {
        return PlayerPrefs.GetInt(
            GetAchievementRewardKey(index),
            0
        ) == 1;
    }


    public static void MarkAchievementRewardClaimed(
        int index
    )
    {
        string key =
            GetAchievementRewardKey(
                index
            );


        if (string.IsNullOrEmpty(key))
        {
            return;
        }


        PlayerPrefs.SetInt(
            key,
            1
        );


        PlayerPrefs.Save();
    }


    private static string GetAchievementRewardKey(
        int index
    )
    {
        switch (index)
        {
            case 0:
                return AchievementReward1ClaimedKey;

            case 1:
                return AchievementReward2ClaimedKey;

            case 2:
                return AchievementReward3ClaimedKey;
        }


        return "";
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
            DailyReward1ClaimedKey
        );

        PlayerPrefs.DeleteKey(
            DailyReward2ClaimedKey
        );

        PlayerPrefs.DeleteKey(
            DailyReward3ClaimedKey
        );


        PlayerPrefs.DeleteKey(
            AchievementReward1ClaimedKey
        );

        PlayerPrefs.DeleteKey(
            AchievementReward2ClaimedKey
        );

        PlayerPrefs.DeleteKey(
            AchievementReward3ClaimedKey
        );


        PlayerPrefs.Save();
    }
}