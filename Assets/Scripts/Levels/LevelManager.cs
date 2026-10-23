using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


public class LevelManager :
    MonoBehaviour
{
    // =====================================================
    // LEVEL
    // =====================================================

    [Header("Fallback Level")]

    [SerializeField]
    private LevelData fallbackLevel;


    [Header("Level Database")]

    [SerializeField]
    private LevelDatabase levelDatabase;


    // =====================================================
    // GAMEPLAY
    // =====================================================

    [Header("Gameplay")]

    [SerializeField]
    private AdaptivePiecePanelLayout
        adaptivePiecePanelLayout;


    [SerializeField]
    private AdaptiveBoardLayout
        adaptiveBoardLayout;


    [SerializeField]
    private GridManager gridManager;


    [SerializeField]
    private HintManager hintManager;


    [SerializeField]
    private PieceSpawner pieceSpawner;


    [SerializeField]
    private GameplayHintController
        gameplayHintController;


    // =====================================================
    // UI
    // =====================================================

    [Header("UI")]

    [SerializeField]
    private TMP_Text levelTitle;


    // =====================================================
    // REWARD
    // =====================================================

    [Header("Level Reward")]

    [SerializeField]
    [Min(0)]
    private int completionRewardCoins =
        25;


    // =====================================================
    // SCENES
    // =====================================================

    [Header("Scenes")]

    [SerializeField]
    private string homeSceneName =
        "LevelSelect";


    // =====================================================
    // STATE
    // =====================================================

    private LevelData currentLevel;


    private bool
        baseRewardGrantedThisRun;


    // =====================================================
    // PUBLIC
    // =====================================================

    public LevelData CurrentLevel =>
        currentLevel;


    public int CompletionRewardCoins =>
        Mathf.Max(
            0,
            completionRewardCoins
        );


    public int TripleRewardTotalCoins =>
        CompletionRewardCoins *
        3;


    public int TripleRewardBonusCoins =>
        CompletionRewardCoins *
        2;


    // =====================================================
    // START
    // =====================================================

    private void Start()
    {
        LoadCurrentLevel();
    }


    // =====================================================
    // LOAD
    // =====================================================

    public void LoadCurrentLevel()
    {
        baseRewardGrantedThisRun =
            false;


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
                "LevelManager: Invalid LevelData. " +
                errorMessage
            );

            return;
        }


        if (adaptiveBoardLayout == null)
        {
            Debug.LogError(
                "LevelManager: AdaptiveBoardLayout " +
                "is not assigned."
            );

            return;
        }


        if (adaptivePiecePanelLayout == null)
        {
            Debug.LogError(
                "LevelManager: AdaptivePiecePanelLayout " +
                "is not assigned."
            );

            return;
        }


        if (gridManager == null)
        {
            Debug.LogError(
                "LevelManager: GridManager " +
                "is not assigned."
            );

            return;
        }


        if (hintManager == null)
        {
            Debug.LogError(
                "LevelManager: HintManager " +
                "is not assigned."
            );

            return;
        }


        if (pieceSpawner == null)
        {
            Debug.LogError(
                "LevelManager: PieceSpawner " +
                "is not assigned."
            );

            return;
        }


        Canvas.ForceUpdateCanvases();


        adaptiveBoardLayout.ApplyLayout(
            currentLevel.Columns,
            currentLevel.Rows
        );


        gridManager.Initialize(
            currentLevel.Columns,
            currentLevel.Rows,
            currentLevel.BlockedCells
        );


        adaptivePiecePanelLayout
            .ApplyLayout();


        hintManager.Initialize(
            currentLevel
        );


        pieceSpawner.BuildPieces(
            currentLevel.Pieces
        );


        if (
            gameplayHintController !=
            null
        )
        {
            gameplayHintController
                .Initialize(
                    currentLevel
                );
        }


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

    public int CompleteCurrentLevel()
    {
        if (currentLevel == null)
        {
            return 0;
        }


        bool alreadyCompleted =
            ProgressManager
                .IsLevelCompleted(
                    currentLevel.LevelNumber
                );


        ProgressManager.CompleteLevel(
            currentLevel.LevelNumber
        );


        // Дневное задание:
        // успешное прохождение уровня.
        PlayerStats
            .RecordLevelCompletedToday();


        int rewardGranted =
            0;


        // Базовая награда выдаётся
        // только за первое прохождение.
        if (!alreadyCompleted)
        {
            rewardGranted =
                CompletionRewardCoins;


            if (rewardGranted > 0)
            {
                PlayerResources.AddCoins(
                    rewardGranted
                );


                baseRewardGrantedThisRun =
                    true;
            }


            Debug.Log(
                $"Level reward: " +
                $"+{rewardGranted} coins."
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


        return rewardGranted;
    }


    // =====================================================
    // TRIPLE REWARD
    // =====================================================

    public bool CanClaimTripleReward()
    {
        if (
            currentLevel == null ||
            !baseRewardGrantedThisRun
        )
        {
            return false;
        }


        return
            !ProgressManager
                .IsTripleRewardClaimed(
                    currentLevel.LevelNumber
                );
    }


    public bool TryGrantTripleRewardBonus()
    {
        if (!CanClaimTripleReward())
        {
            return false;
        }


        int bonus =
            TripleRewardBonusCoins;


        if (bonus > 0)
        {
            PlayerResources.AddCoins(
                bonus
            );
        }


        ProgressManager
            .MarkTripleRewardClaimed(
                currentLevel.LevelNumber
            );


        Debug.Log(
            $"Triple reward bonus: " +
            $"+{bonus} coins. " +
            $"Total level reward: " +
            $"{TripleRewardTotalCoins}."
        );


        return true;
    }


    // =====================================================
    // CONTINUE
    // =====================================================

    public void ContinueAfterCompletion()
    {
        if (HasNextLevel())
        {
            LoadNextLevel();

            return;
        }


        LevelSelectionState
            .ClearSelection();


        SceneManager.LoadScene(
            homeSceneName
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
            SceneManager
                .GetActiveScene();


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