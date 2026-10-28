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


    public int Count =>
        levels == null
            ? 0
            : levels.Length;


    // =====================================================
    // GET
    // =====================================================

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
        if (
            levels == null ||
            levels.Length == 0
        )
        {
            return null;
        }


        return levels[0];
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


    // =====================================================
    // NEXT
    // =====================================================

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


            int nextIndex =
                i + 1;


            if (
                nextIndex >=
                levels.Length
            )
            {
                return null;
            }


            return
                levels[
                    nextIndex
                ];
        }


        return null;
    }


    // =====================================================
    // VALIDATION
    // =====================================================

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


            int expectedNumber =
                i + 1;


            if (
                level.LevelNumber !=
                expectedNumber
            )
            {
                errorMessage =
                    $"Element {i} must be " +
                    $"Level {expectedNumber}, " +
                    $"but contains Level " +
                    $"{level.LevelNumber}.";

                return false;
            }


            if (
                !level.IsValid(
                    out string levelError
                )
            )
            {
                errorMessage =
                    $"{level.name}: " +
                    levelError;

                return false;
            }
        }


        errorMessage =
            "";

        return true;
    }
}