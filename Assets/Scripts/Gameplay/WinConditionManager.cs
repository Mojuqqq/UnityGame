using UnityEngine;

public class WinConditionManager :
    MonoBehaviour
{
    [Header("Gameplay")]

    [SerializeField]
    private GridManager gridManager;

    [SerializeField]
    private HintManager hintManager;

    [SerializeField]
    private LevelManager levelManager;


    [Header("Complete UI")]

    [SerializeField]
    private GameObject
        levelCompleteOverlay;

    [SerializeField]
    private GameObject
        nextLevelButton;


    private bool levelCompleted;


    private void OnEnable()
    {
        if (gridManager != null)
        {
            gridManager.GridChanged +=
                CheckWinCondition;
        }
    }


    private void OnDisable()
    {
        if (gridManager != null)
        {
            gridManager.GridChanged -=
                CheckWinCondition;
        }
    }


    private void Start()
    {
        levelCompleted =
            false;


        if (
            levelCompleteOverlay !=
            null
        )
        {
            levelCompleteOverlay
                .SetActive(false);
        }
    }


    private void CheckWinCondition()
    {
        if (levelCompleted)
        {
            return;
        }


        if (hintManager == null)
        {
            Debug.LogError(
                "WinConditionManager: HintManager is not assigned."
            );

            return;
        }


        if (!hintManager.IsSolved())
        {
            return;
        }


        CompleteLevel();
    }


    private void CompleteLevel()
    {
        levelCompleted =
            true;


        if (levelManager != null)
        {
            levelManager
                .CompleteCurrentLevel();
        }


        if (nextLevelButton != null)
        {
            bool hasNextLevel =
                levelManager != null &&
                levelManager.HasNextLevel();


            nextLevelButton.SetActive(
                hasNextLevel
            );
        }


        if (
            levelCompleteOverlay !=
            null
        )
        {
            levelCompleteOverlay
                .SetActive(true);
        }


        Debug.Log(
            "Level completed!"
        );
    }
}