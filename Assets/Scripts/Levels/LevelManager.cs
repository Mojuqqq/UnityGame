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

    [SerializeField]
    private GameplayHintController gameplayHintController;


    [Header("UI")]

    [SerializeField]
    private TMP_Text levelTitle;


    [Header("Rewards")]

    [SerializeField]
    [Min(0)]
    private int coinsPerLevel = 100;


    private LevelData currentLevel;


    public LevelData CurrentLevel =>
        currentLevel;


    private void Start()
    {
        LoadCurrentLevel();
    }


    // =====================================================
    // LOAD
    // =====================================================

    public void LoadCurrentLevel()
    {
        currentLevel =
            LevelSelectionState.SelectedLevel;


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
                "LevelManager: Invalid LevelData. " +
                errorMessage
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


        if (adaptivePiecePanelLayout == null)
        {
            Debug.LogError(
                "LevelManager: AdaptivePiecePanelLayout is not assigned."
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


        Canvas.ForceUpdateCanvases();


        // =================================================
        // 1. BOARD SIZE
        // =================================================

        adaptiveBoardLayout.ApplyLayout(
            currentLevel.Columns,
            currentLevel.Rows
        );


        // =================================================
        // 2. GRID
        // =================================================

        gridManager.Initialize(
            currentLevel.Columns,
            currentLevel.Rows,
            currentLevel.BlockedCells
        );


        // =================================================
        // 3. PIECE PANEL
        // =================================================

        adaptivePiecePanelLayout.ApplyLayout();


        // =================================================
        // 4. ROW / COLUMN HINTS
        // =================================================

        hintManager.Initialize(
            currentLevel
        );


        // =================================================
        // 5. PIECES
        // =================================================

        pieceSpawner.BuildPieces(
            currentLevel.Pieces
        );


        // =================================================
        // 6. GAMEPLAY HINTS
        // =================================================

        if (gameplayHintController != null)
        {
            gameplayHintController.Initialize(
                currentLevel
            );
        }


        // =================================================
        // 7. TITLE
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


    // =====================================================
    // COMPLETE
    // =====================================================

    public void CompleteCurrentLevel()
    {
        if (currentLevel == null)
        {
            return;
        }


        bool alreadyCompleted =
            ProgressManager.IsLevelCompleted(
                currentLevel.LevelNumber
            );


        ProgressManager.CompleteLevel(
            currentLevel.LevelNumber
        );


        // Для ежедневного задания считаем
        // каждое успешное прохождение.
        PlayerStats.RecordLevelCompletedToday();


        // Монеты выдаём только
        // за первое прохождение уровня.
        if (!alreadyCompleted)
        {
            PlayerResources.AddCoins(
                coinsPerLevel
            );


            Debug.Log(
                $"Level reward: +{coinsPerLevel} coins."
            );
        }


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


    // =====================================================
    // NEXT LEVEL
    // =====================================================

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