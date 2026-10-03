using UnityEngine;

public static class ProgressManager
{
    private const string UnlockedPrefix =
        "Puzzle_Level_Unlocked_";

    private const string CompletedPrefix =
        "Puzzle_Level_Completed_";


    // =====================================================
    // STATE
    // =====================================================

    public static bool IsLevelUnlocked(
        int levelNumber
    )
    {
        return
            PlayerPrefs.GetInt(
                UnlockedPrefix +
                levelNumber,
                0
            ) == 1;
    }


    public static bool IsLevelCompleted(
        int levelNumber
    )
    {
        return
            PlayerPrefs.GetInt(
                CompletedPrefix +
                levelNumber,
                0
            ) == 1;
    }


    // =====================================================
    // CHANGE STATE
    // =====================================================

    public static void UnlockLevel(
        int levelNumber
    )
    {
        if (levelNumber <= 0)
        {
            return;
        }


        PlayerPrefs.SetInt(
            UnlockedPrefix +
            levelNumber,
            1
        );


        PlayerPrefs.Save();
    }


    public static void CompleteLevel(
        int levelNumber
    )
    {
        if (levelNumber <= 0)
        {
            return;
        }


        PlayerPrefs.SetInt(
            CompletedPrefix +
            levelNumber,
            1
        );


        PlayerPrefs.Save();
    }


    // =====================================================
    // CURRENT PLAYABLE LEVEL
    // =====================================================

    public static LevelData GetCurrentPlayableLevel(
        LevelDatabase database
    )
    {
        if (
            database == null ||
            database.Levels == null
        )
        {
            return null;
        }


        // Первый непройденный уровень
        // становится текущим.
        foreach (
            LevelData level
            in database.Levels
        )
        {
            if (level == null)
            {
                continue;
            }


            if (
                !IsLevelCompleted(
                    level.LevelNumber
                )
            )
            {
                // Поддерживаем старую систему unlocked,
                // хотя выбирать уровни через UI
                // теперь больше нельзя.
                UnlockLevel(
                    level.LevelNumber
                );


                return level;
            }
        }


        // null = все уровни пройдены.
        return null;
    }


    // =====================================================
    // PROGRESS
    // =====================================================

    public static int GetCompletedLevelCount(
        LevelDatabase database
    )
    {
        if (
            database == null ||
            database.Levels == null
        )
        {
            return 0;
        }


        int count = 0;


        foreach (
            LevelData level
            in database.Levels
        )
        {
            if (level == null)
            {
                continue;
            }


            if (
                IsLevelCompleted(
                    level.LevelNumber
                )
            )
            {
                count++;
            }
        }


        return count;
    }


    public static bool AreAllLevelsCompleted(
        LevelDatabase database
    )
    {
        if (
            database == null ||
            database.Count == 0
        )
        {
            return false;
        }


        return
            GetCompletedLevelCount(
                database
            )
            >=
            database.Count;
    }


    // =====================================================
    // RESET
    // =====================================================

    public static void ResetProgress(
        LevelDatabase database
    )
    {
        if (
            database == null ||
            database.Levels == null
        )
        {
            return;
        }


        foreach (
            LevelData level
            in database.Levels
        )
        {
            if (level == null)
            {
                continue;
            }


            PlayerPrefs.DeleteKey(
                UnlockedPrefix +
                level.LevelNumber
            );


            PlayerPrefs.DeleteKey(
                CompletedPrefix +
                level.LevelNumber
            );
        }


        PlayerPrefs.Save();
    }
}