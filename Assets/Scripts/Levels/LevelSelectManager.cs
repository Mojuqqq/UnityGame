using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectManager :
    MonoBehaviour
{
    [Header("Levels")]

    [SerializeField]
    private LevelDatabase levelDatabase;


    [Header("Current Level UI")]

    [SerializeField]
    private TMP_Text levelText;

    [SerializeField]
    private TMP_Text progressText;


    [Header("Play")]

    [SerializeField]
    private Button playButton;

    [SerializeField]
    private TMP_Text playButtonText;


    [Header("Resources")]

    [SerializeField]
    private TMP_Text hintCountText;


    [Header("Popups")]

    [SerializeField]
    private GameObject achievementsPopup;

    [SerializeField]
    private GameObject dailyTasksPopup;


    [Header("Scenes")]

    [SerializeField]
    private string gameSceneName =
        "Level01";


    private LevelData currentPlayableLevel;


    // =====================================================
    // START
    // =====================================================

    private void Start()
    {
        if (levelDatabase == null)
        {
            Debug.LogError(
                "LevelSelectManager: " +
                "LevelDatabase is not assigned."
            );

            return;
        }


        if (
            !levelDatabase.IsValid(
                out string errorMessage
            )
        )
        {
            Debug.LogError(
                $"LevelSelectManager: " +
                $"Invalid LevelDatabase. " +
                $"{errorMessage}"
            );

            return;
        }


        HideAllPopups();

        RefreshMenu();
    }


    // =====================================================
    // REFRESH
    // =====================================================

    public void RefreshMenu()
    {
        currentPlayableLevel =
            ProgressManager
                .GetCurrentPlayableLevel(
                    levelDatabase
                );


        RefreshLevelInfo();

        RefreshHints();
    }


    private void RefreshLevelInfo()
    {
        int completed =
            ProgressManager
                .GetCompletedLevelCount(
                    levelDatabase
                );


        if (progressText != null)
        {
            progressText.text =
                $"{completed} / " +
                $"{levelDatabase.Count}";
        }


        // =================================================
        // ALL LEVELS COMPLETED
        // =================================================

        if (currentPlayableLevel == null)
        {
            if (levelText != null)
            {
                levelText.text =
                    "ВСЕ УРОВНИ ПРОЙДЕНЫ";
            }


            if (playButton != null)
            {
                playButton.interactable =
                    false;
            }


            if (playButtonText != null)
            {
                playButtonText.text =
                    "ГОТОВО";
            }


            return;
        }


        // =================================================
        // CURRENT LEVEL
        // =================================================

        if (levelText != null)
        {
            if (
                !string.IsNullOrWhiteSpace(
                    currentPlayableLevel
                        .DisplayName
                )
            )
            {
                levelText.text =
                    currentPlayableLevel
                        .DisplayName;
            }
            else
            {
                levelText.text =
                    $"УРОВЕНЬ " +
                    $"{currentPlayableLevel.LevelNumber}";
            }
        }


        if (playButton != null)
        {
            playButton.interactable =
                true;
        }


        if (playButtonText != null)
        {
            playButtonText.text =
                "ИГРАТЬ";
        }
    }


    private void RefreshHints()
    {
        if (hintCountText == null)
        {
            return;
        }


        hintCountText.text =
            PlayerResources
                .GetHints()
                .ToString();
    }


    // =====================================================
    // PLAY
    // =====================================================

    public void PlayCurrentLevel()
    {
        if (currentPlayableLevel == null)
        {
            return;
        }


        LevelSelectionState.SelectLevel(
            currentPlayableLevel
        );


        SceneManager.LoadScene(
            gameSceneName
        );
    }


    // =====================================================
    // ACHIEVEMENTS
    // =====================================================

    public void OpenAchievements()
    {
        HideAllPopups();


        if (achievementsPopup != null)
        {
            achievementsPopup.SetActive(
                true
            );
        }
    }


    // =====================================================
    // DAILY TASKS
    // =====================================================

    public void OpenDailyTasks()
    {
        HideAllPopups();


        if (dailyTasksPopup != null)
        {
            dailyTasksPopup.SetActive(
                true
            );
        }
    }


    // =====================================================
    // POPUPS
    // =====================================================

    public void ClosePopups()
    {
        HideAllPopups();
    }


    private void HideAllPopups()
    {
        if (achievementsPopup != null)
        {
            achievementsPopup.SetActive(
                false
            );
        }


        if (dailyTasksPopup != null)
        {
            dailyTasksPopup.SetActive(
                false
            );
        }
    }


    // =====================================================
    // TESTING
    // =====================================================

    [ContextMenu(
        "Reset Progress For Testing"
    )]
    private void ResetProgressForTesting()
    {
        ProgressManager.ResetProgress(
            levelDatabase
        );


        LevelSelectionState
            .ClearSelection();


        RefreshMenu();


        Debug.Log(
            "Level progress reset."
        );
    }


    [ContextMenu(
        "Reset All Player Data For Testing"
    )]
    private void ResetAllPlayerDataForTesting()
    {
        ProgressManager.ResetProgress(
            levelDatabase
        );


        PlayerResources
            .ResetForTesting();


        LevelSelectionState
            .ClearSelection();


        RefreshMenu();


        Debug.Log(
            "Player test data reset."
        );
    }
}