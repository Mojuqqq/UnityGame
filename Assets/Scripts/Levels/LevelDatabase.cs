using UnityEngine;

[CreateAssetMenu(
    fileName = "LevelDatabase",
    menuName = "Puzzle/Level Database"
)]
public class LevelDatabase :
    ScriptableObject
{
    [SerializeField]
    private LevelData[] levels;


    public LevelData[] Levels =>
        levels;


    public int Count
    {
        get
        {
            if (levels == null)
            {
                return 0;
            }

            return levels.Length;
        }
    }


    public LevelData GetLevel(
        int index
    )
    {
        if (
            levels == null ||
            index < 0 ||
            index >= levels.Length
        )
        {
            return null;
        }

        return levels[index];
    }


    public LevelData GetFirstLevel()
    {
        if (levels == null)
        {
            return null;
        }

        foreach (
            LevelData level
            in levels
        )
        {
            if (level != null)
            {
                return level;
            }
        }

        return null;
    }


    public LevelData GetLevelByNumber(
        int levelNumber
    )
    {
        if (levels == null)
        {
            return null;
        }

        foreach (
            LevelData level
            in levels
        )
        {
            if (
                level != null &&
                level.LevelNumber ==
                levelNumber
            )
            {
                return level;
            }
        }

        return null;
    }


    public LevelData GetNextLevel(
        LevelData currentLevel
    )
    {
        if (
            levels == null ||
            currentLevel == null
        )
        {
            return null;
        }

        for (
            int i = 0;
            i < levels.Length;
            i++
        )
        {
            if (
                levels[i] !=
                currentLevel
            )
            {
                continue;
            }

            for (
                int nextIndex = i + 1;
                nextIndex < levels.Length;
                nextIndex++
            )
            {
                if (
                    levels[nextIndex] !=
                    null
                )
                {
                    return
                        levels[nextIndex];
                }
            }

            return null;
        }

        return null;
    }

    public bool IsValid(
    out string errorMessage
)
{
    if (
        levels == null ||
        levels.Length == 0
    )
    {
        errorMessage =
            "Level Database is empty.";

        return false;
    }


    for (
        int i = 0;
        i < levels.Length;
        i++
    )
    {
        LevelData level =
            levels[i];


        if (level == null)
        {
            errorMessage =
                $"Level element {i} is empty.";

            return false;
        }


        if (
            !level.IsValid(
                out string levelError
            )
        )
        {
            errorMessage =
                $"{level.name}: {levelError}";

            return false;
        }


        for (
            int j = i + 1;
            j < levels.Length;
            j++
        )
        {
            if (
                levels[j] != null &&
                levels[j].LevelNumber ==
                level.LevelNumber
            )
            {
                errorMessage =
                    $"Duplicate Level Number: " +
                    $"{level.LevelNumber}.";

                return false;
            }
        }
    }


    errorMessage = "";

    return true;
}
}