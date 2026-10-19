using System;
using UnityEngine;


public enum TaskCategory
{
    Achievement,
    Daily
}


public enum TaskCondition
{
    TotalPiecesPlaced,
    LevelsCompleted,
    TotalHintsUsed,

    DailyLevelsCompleted,
    DailyHintsUsed
}


[Serializable]
public class TaskDefinition
{
    // =====================================================
    // ID
    // =====================================================

    [Header("Identity")]

    [SerializeField]
    private string id;


    [SerializeField]
    private string title;


    // =====================================================
    // TYPE
    // =====================================================

    [Header("Type")]

    [SerializeField]
    private TaskCategory category;


    [SerializeField]
    private TaskCondition condition;


    // =====================================================
    // GOAL
    // =====================================================

    [Header("Goal")]

    [SerializeField]
    [Min(1)]
    private int target =
        1;


    // =====================================================
    // REWARD
    // =====================================================

    [Header("Reward")]

    [SerializeField]
    [Min(0)]
    private int rewardCoins =
        25;


    // =====================================================
    // PUBLIC DATA
    // =====================================================

    public string Id
    {
        get
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return "";
            }

            return id.Trim();
        }
    }


    public string Title =>
        title;


    public TaskCategory Category =>
        category;


    public TaskCondition Condition =>
        condition;


    public int Target =>
        Mathf.Max(
            1,
            target
        );


    public int RewardCoins =>
        Mathf.Max(
            0,
            rewardCoins
        );


    // =====================================================
    // VALIDATION
    // =====================================================

    public bool IsValid(
        out string errorMessage
    )
    {
        if (string.IsNullOrWhiteSpace(Id))
        {
            errorMessage =
                "Task ID is empty.";

            return false;
        }


        if (Id.Contains("|"))
        {
            errorMessage =
                $"Task '{Id}' contains forbidden character |.";

            return false;
        }


        if (string.IsNullOrWhiteSpace(title))
        {
            errorMessage =
                $"Task '{Id}' has no title.";

            return false;
        }


        if (target <= 0)
        {
            errorMessage =
                $"Task '{Id}' target must be greater than 0.";

            return false;
        }


        if (rewardCoins < 0)
        {
            errorMessage =
                $"Task '{Id}' reward cannot be negative.";

            return false;
        }


        errorMessage =
            "";

        return true;
    }
}