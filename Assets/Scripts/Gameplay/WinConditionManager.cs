using UnityEngine;

public class WinConditionManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private GridManager gridManager;

    [SerializeField]
    private HintManager hintManager;

    [SerializeField]
    private GameObject levelCompleteOverlay;

    private bool levelCompleted = false;


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
        levelCompleted = false;

        if (levelCompleteOverlay != null)
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
        levelCompleted = true;

        Debug.Log(
            "Level completed!"
        );

        if (levelCompleteOverlay != null)
        {
            levelCompleteOverlay
                .SetActive(true);
        }
    }
}