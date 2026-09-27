using UnityEngine;

public static class ProgressManager
{
    private const string UnlockedPrefix =
        "Puzzle_Level_Unlocked_";

    private const string CompletedPrefix =
        "Puzzle_Level_Completed_";


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


    public static void ResetProgress(
        LevelDatabase database
    )
    {
        if (database == null)
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