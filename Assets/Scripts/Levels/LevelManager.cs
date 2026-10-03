using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [Header("Fallback Level")]

    [SerializeField]
    private LevelData fallbackLevel;


    [Header("Level Database")]

    [SerializeField]
    private LevelDatabase levelDatabase;


    [Header("Gameplay")]

    [SerializeField]
    private AdaptivePiecePanelLayout adaptivePiecePanelLayout;

    [SerializeField]
    private AdaptiveBoardLayout adaptiveBoardLayout;

    [SerializeField]
    private GridManager gridManager;

    [SerializeField]
    private HintManager hintManager;

    [SerializeField]
    private PieceSpawner pieceSpawner;


    [Header("UI")]

    [SerializeField]
    private TMP_Text levelTitle;


    private LevelData currentLevel;


    public LevelData CurrentLevel =>
        currentLevel;


    private void Start()
    {
        LoadCurrentLevel();
    }


    public void LoadCurrentLevel()
    {
        currentLevel =
            LevelSelectionState
                .SelectedLevel;


        if (currentLevel == null)
        {
            currentLevel =
                fallbackLevel;
        }


        if (currentLevel == null)
        {
            Debug.LogError(
                "LevelManager: No level selected " +
                "and no fallback level assigned."
            );

            return;
        }


        if (
            !currentLevel.IsValid(
                out string errorMessage
            )
        )
        {
            Debug.LogError(
                $"LevelManager: Invalid LevelData. " +
                $"{errorMessage}"
            );

            return;
        }


        if (adaptiveBoardLayout == null)
        {
            Debug.LogError(
                "LevelManager: AdaptiveBoardLayout is not assigned."
            );

            return;
        }


        if (gridManager == null)
        {
            Debug.LogError(
                "LevelManager: GridManager is not assigned."
            );

            return;
        }


        if (hintManager == null)
        {
            Debug.LogError(
                "LevelManager: HintManager is not assigned."
            );

            return;
        }


        if (pieceSpawner == null)
        {
            Debug.LogError(
                "LevelManager: PieceSpawner is not assigned."
            );

            return;
        }


        // =================================================
        // 1. CANVAS
        // =================================================

       Canvas.ForceUpdateCanvases();


// 1. Размер Board.
adaptiveBoardLayout.ApplyLayout(
    currentLevel.Columns,
    currentLevel.Rows
);


gridManager.Initialize(
    currentLevel.Columns,
    currentLevel.Rows,
    currentLevel.BlockedCells
);


adaptivePiecePanelLayout.ApplyLayout();


hintManager.Initialize(
    currentLevel
);


pieceSpawner.BuildPieces(
    currentLevel.Pieces
);


        // =================================================
        // 6. TITLE
        // =================================================

        if (levelTitle != null)
        {
            levelTitle.text =
                currentLevel.DisplayName;
        }


        Debug.Log(
            $"Loaded level " +
            $"{currentLevel.LevelNumber}: " +
            $"{currentLevel.DisplayName}"
        );
    }


    public void CompleteCurrentLevel()
    {
        if (currentLevel == null)
        {
            return;
        }


        ProgressManager.CompleteLevel(
            currentLevel.LevelNumber
        );


        LevelData nextLevel =
            GetNextLevel();


        if (nextLevel != null)
        {
            ProgressManager.UnlockLevel(
                nextLevel.LevelNumber
            );
        }


        Debug.Log(
            $"Completed level " +
            $"{currentLevel.LevelNumber}"
        );
    }


    public bool HasNextLevel()
    {
        return
            GetNextLevel() != null;
    }


    public void LoadNextLevel()
    {
        LevelData nextLevel =
            GetNextLevel();


        if (nextLevel == null)
        {
            return;
        }


        ProgressManager.UnlockLevel(
            nextLevel.LevelNumber
        );


        LevelSelectionState.SelectLevel(
            nextLevel
        );


        Scene currentScene =
            SceneManager.GetActiveScene();


        SceneManager.LoadScene(
            currentScene.name
        );
    }


    private LevelData GetNextLevel()
    {
        if (
            levelDatabase == null ||
            currentLevel == null
        )
        {
            return null;
        }


        return
            levelDatabase.GetNextLevel(
                currentLevel
            );
    }
}