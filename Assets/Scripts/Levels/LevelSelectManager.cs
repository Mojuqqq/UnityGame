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


    [Header("Achievements")]

    [SerializeField]
    private GameObject achievementsPopup;

    [SerializeField]
    private TMP_Text achievementsBodyText;

    [SerializeField]
    private GameObject achievementsBadge;

    [SerializeField]
    private TMP_Text achievementsBadgeText;


    [Header("Daily Tasks")]

    [SerializeField]
    private GameObject dailyTasksPopup;

    [SerializeField]
    private TMP_Text dailyTasksBodyText;

    [SerializeField]
    private GameObject dailyTasksBadge;

    [SerializeField]
    private TMP_Text dailyTasksBadgeText;


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
                "LevelSelectManager: " +
                "Invalid LevelDatabase. " +
                errorMessage
            );

            return;
        }


        PlayerStats.EnsureDailyReset();


        HideAllPopups();

        RefreshMenu();
    }


    // =====================================================
    // REFRESH
    // =====================================================

    public void RefreshMenu()
    {
        PlayerStats.EnsureDailyReset();


        currentPlayableLevel =
            ProgressManager
                .GetCurrentPlayableLevel(
                    levelDatabase
                );


        RefreshLevelInfo();

        RefreshAchievements();

        RefreshDailyTasks();

        RefreshBadges();
    }


    // =====================================================
    // CURRENT LEVEL
    // =====================================================

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
                $"{completed} / {levelDatabase.Count}";
        }


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


        if (levelText != null)
        {
            if (
                !string.IsNullOrWhiteSpace(
                    currentPlayableLevel.DisplayName
                )
            )
            {
                levelText.text =
                    currentPlayableLevel.DisplayName;
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

        RefreshAchievements();

        RefreshBadges();


        if (achievementsPopup != null)
        {
            achievementsPopup.SetActive(
                true
            );
        }
    }


    private void RefreshAchievements()
    {
        if (achievementsBodyText == null)
        {
            return;
        }


        int piecesPlaced =
            PlayerStats
                .GetTotalPiecesPlaced();


        int levelsCompleted =
            ProgressManager
                .GetCompletedLevelCount(
                    levelDatabase
                );


        int hintsUsed =
            PlayerStats
                .GetTotalHintsUsed();


        achievementsBodyText.text =
            BuildProgressLine(
                "Выставить 5 фигур",
                piecesPlaced,
                5
            )
            +
            "\n\n"
            +
            BuildProgressLine(
                "Пройти 3 уровня",
                levelsCompleted,
                3
            )
            +
            "\n\n"
            +
            BuildProgressLine(
                "Использовать 3 подсказки",
                hintsUsed,
                3
            );
    }


    private int GetCompletedAchievementCount()
    {
        int completed =
            0;


        if (
            PlayerStats.GetTotalPiecesPlaced()
            >=
            5
        )
        {
            completed++;
        }


        if (
            ProgressManager
                .GetCompletedLevelCount(
                    levelDatabase
                )
            >=
            3
        )
        {
            completed++;
        }


        if (
            PlayerStats.GetTotalHintsUsed()
            >=
            3
        )
        {
            completed++;
        }


        return completed;
    }


    // =====================================================
    // DAILY TASKS
    // =====================================================

    public void OpenDailyTasks()
    {
        HideAllPopups();

        RefreshDailyTasks();

        RefreshBadges();


        if (dailyTasksPopup != null)
        {
            dailyTasksPopup.SetActive(
                true
            );
        }
    }


    private void RefreshDailyTasks()
    {
        if (dailyTasksBodyText == null)
        {
            return;
        }


        PlayerStats.EnsureDailyReset();


        int levelsToday =
            PlayerStats
                .GetDailyLevelsCompleted();


        int hintsToday =
            PlayerStats
                .GetDailyHintsUsed();


        dailyTasksBodyText.text =
            BuildProgressLine(
                "Пройти 1 уровень",
                levelsToday,
                1
            )
            +
            "\n\n"
            +
            BuildProgressLine(
                "Пройти 3 уровня",
                levelsToday,
                3
            )
            +
            "\n\n"
            +
            BuildProgressLine(
                "Использовать 1 подсказку",
                hintsToday,
                1
            );
    }


    // =====================================================
    // PROGRESS TEXT
    // =====================================================

    private string BuildProgressLine(
        string title,
        int current,
        int target
    )
    {
        int visibleCurrent =
            Mathf.Min(
                current,
                target
            );


        bool completed =
            current >=
            target;


        string state =
            completed
                ? "✓"
                : "○";


        return
            $"{state} {title}\n" +
            $"{visibleCurrent} / {target}";
    }


    // =====================================================
    // BADGES
    // =====================================================

    private void RefreshBadges()
    {
        int completedAchievements =
            GetCompletedAchievementCount();


        int completedDailyTasks =
            PlayerStats
                .GetCompletedDailyTaskCount();


        SetBadge(
            achievementsBadge,
            achievementsBadgeText,
            completedAchievements
        );


        SetBadge(
            dailyTasksBadge,
            dailyTasksBadgeText,
            completedDailyTasks
        );
    }


    private void SetBadge(
        GameObject badge,
        TMP_Text badgeText,
        int value
    )
    {
        if (badge == null)
        {
            return;
        }


        bool show =
            value > 0;


        badge.SetActive(
            show
        );


        if (
            show &&
            badgeText != null
        )
        {
            badgeText.text =
                value.ToString();
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


        PlayerStats
            .ResetForTesting();


        LevelSelectionState
            .ClearSelection();


        RefreshMenu();


        Debug.Log(
            "Player data reset."
        );
    }
}