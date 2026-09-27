using UnityEngine;

[CreateAssetMenu(
    fileName = "LevelDatabase",
    menuName = "Puzzle/Level Database"
)]
public class LevelDatabase : ScriptableObject
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


    public LevelData GetLevelByNumber(
        int levelNumber
    )
    {
        if (levels == null)
        {
            return null;
        }

        foreach (
            LevelData level in levels
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
}