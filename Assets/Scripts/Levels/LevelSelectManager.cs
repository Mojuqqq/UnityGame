using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class LevelSelectManager :
    MonoBehaviour
{
    // =====================================================
    // LEVELS
    // =====================================================

    [Header("Levels")]

    [SerializeField]
    private LevelDatabase levelDatabase;


    // =====================================================
    // CURRENT LEVEL UI
    // =====================================================

    [Header("Current Level UI")]

    [SerializeField]
    private TMP_Text levelText;

    [SerializeField]
    private TMP_Text progressText;


    // =====================================================
    // PLAY
    // =====================================================

    [Header("Play")]

    [SerializeField]
    private Button playButton;

    [SerializeField]
    private TMP_Text playButtonText;


    // =====================================================
    // TASK SYSTEM
    // =====================================================

    [Header("Task System")]

    [SerializeField]
    private TaskDatabase taskDatabase;

    [SerializeField]
    private TaskItemView taskItemPrefab;


    // =====================================================
    // ACHIEVEMENTS
    // =====================================================

    [Header("Achievements")]

    [SerializeField]
    private GameObject achievementsPopup;

    [SerializeField]
    private Transform achievementsContent;

    [SerializeField]
    private GameObject achievementsBadge;

    [SerializeField]
    private TMP_Text achievementsBadgeText;


    // =====================================================
    // DAILY TASKS
    // =====================================================

    [Header("Daily Tasks")]

    [SerializeField]
    private GameObject dailyTasksPopup;

    [SerializeField]
    private Transform dailyTasksContent;

    [SerializeField]
    private GameObject dailyTasksBadge;

    [SerializeField]
    private TMP_Text dailyTasksBadgeText;


    // =====================================================
    // CURRENCY UI
    // =====================================================

    [Header("Currency UI")]

    [SerializeField]
    private TMP_Text coinsText;


    // =====================================================
    // SCENES
    // =====================================================

    [Header("Scenes")]

    [SerializeField]
    private string gameSceneName =
        "Level01";


    // =====================================================
    // STATE
    // =====================================================

    private LevelData currentPlayableLevel;


    // =====================================================
    // START
    // =====================================================

    private void Start()
    {
        if (!ValidateReferences())
        {
            return;
        }


        PlayerStats.EnsureDailyReset();


        HideAllPopups();

        RefreshMenu();
    }


    // =====================================================
    // VALIDATION
    // =====================================================

    private bool ValidateReferences()
    {
        if (levelDatabase == null)
        {
            Debug.LogError(
                "LevelSelectManager: " +
                "LevelDatabase is not assigned."
            );

            return false;
        }


        if (
            !levelDatabase.IsValid(
                out string levelError
            )
        )
        {
            Debug.LogError(
                "LevelSelectManager: " +
                "Invalid LevelDatabase. " +
                levelError
            );

            return false;
        }


        if (taskDatabase == null)
        {
            Debug.LogError(
                "LevelSelectManager: " +
                "TaskDatabase is not assigned."
            );

            return false;
        }


        if (
            !taskDatabase.IsValid(
                out string taskError
            )
        )
        {
            Debug.LogError(
                "LevelSelectManager: " +
                "Invalid TaskDatabase. " +
                taskError
            );

            return false;
        }


        if (taskItemPrefab == null)
        {
            Debug.LogError(
                "LevelSelectManager: " +
                "TaskItemPrefab is not assigned."
            );

            return false;
        }


        if (achievementsContent == null)
        {
            Debug.LogError(
                "LevelSelectManager: " +
                "Achievements Content is not assigned."
            );

            return false;
        }


        if (dailyTasksContent == null)
        {
            Debug.LogError(
                "LevelSelectManager: " +
                "Daily Tasks Content is not assigned."
            );

            return false;
        }


        return true;
    }


    // =====================================================
    // REFRESH MENU
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

        RefreshCoins();

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
    // COINS
    // =====================================================

    private void RefreshCoins()
    {
        if (coinsText == null)
        {
            return;
        }


        coinsText.text =
            PlayerResources
                .GetCoins()
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
    // OPEN ACHIEVEMENTS
    // =====================================================

    public void OpenAchievements()
    {
        HideAllPopups();


        BuildTaskList(
            TaskCategory.Achievement,
            achievementsContent
        );


        if (achievementsPopup != null)
        {
            achievementsPopup.SetActive(
                true
            );
        }
    }


    // =====================================================
    // OPEN DAILY
    // =====================================================

    public void OpenDailyTasks()
    {
        PlayerStats.EnsureDailyReset();


        HideAllPopups();


        BuildTaskList(
            TaskCategory.Daily,
            dailyTasksContent
        );


        if (dailyTasksPopup != null)
        {
            dailyTasksPopup.SetActive(
                true
            );
        }
    }


    // =====================================================
    // BUILD LIST
    // =====================================================

    private void BuildTaskList(
        TaskCategory category,
        Transform content
    )
    {
        if (content == null)
        {
            return;
        }


        ClearContent(
            content
        );


        foreach (
            TaskDefinition task
            in
            taskDatabase.Tasks
        )
        {
            if (task == null)
            {
                continue;
            }


            if (task.Category != category)
            {
                continue;
            }


            TaskItemView item =
                Instantiate(
                    taskItemPrefab,
                    content
                );


            int currentProgress =
                GetTaskProgress(
                    task
                );


            bool rewardClaimed =
                PlayerStats
                    .IsTaskRewardClaimed(
                        task.Id,
                        task.Category
                    );


            item.Initialize(
                task,
                currentProgress,
                rewardClaimed,
                OnTaskClaimRequested
            );
        }
    }


    // =====================================================
    // CLEAR LIST
    // =====================================================

    private void ClearContent(
        Transform content
    )
    {
        for (
            int i =
                content.childCount - 1;

            i >= 0;

            i--
        )
        {
            GameObject child =
                content
                    .GetChild(i)
                    .gameObject;


            // Скрываем сразу,
            // чтобы не было одного кадра
            // со старыми и новыми карточками.
            child.SetActive(
                false
            );


            Destroy(
                child
            );
        }
    }


    // =====================================================
    // TASK PROGRESS
    // =====================================================

    private int GetTaskProgress(
        TaskDefinition task
    )
    {
        switch (task.Condition)
        {
            case TaskCondition.TotalPiecesPlaced:

                return
                    PlayerStats
                        .GetTotalPiecesPlaced();


            case TaskCondition.LevelsCompleted:

                return
                    ProgressManager
                        .GetCompletedLevelCount(
                            levelDatabase
                        );


            case TaskCondition.TotalHintsUsed:

                return
                    PlayerStats
                        .GetTotalHintsUsed();


            case TaskCondition.DailyLevelsCompleted:

                return
                    PlayerStats
                        .GetDailyLevelsCompleted();


            case TaskCondition.DailyHintsUsed:

                return
                    PlayerStats
                        .GetDailyHintsUsed();
        }


        return 0;
    }


    // =====================================================
    // CLAIM TASK
    // =====================================================

    private void OnTaskClaimRequested(
        TaskDefinition task
    )
    {
        if (task == null)
        {
            return;
        }


        int currentProgress =
            GetTaskProgress(
                task
            );


        if (
            currentProgress <
            task.Target
        )
        {
            return;
        }


        if (
            PlayerStats
                .IsTaskRewardClaimed(
                    task.Id,
                    task.Category
                )
        )
        {
            return;
        }


        PlayerStats
            .MarkTaskRewardClaimed(
                task.Id,
                task.Category
            );


        PlayerResources.AddCoins(
            task.RewardCoins
        );


        RefreshMenu();


        // Обновляем открытый список,
        // чтобы кнопка сразу стала "ПОЛУЧЕНО".
        if (
            task.Category ==
            TaskCategory.Achievement
        )
        {
            BuildTaskList(
                TaskCategory.Achievement,
                achievementsContent
            );
        }
        else
        {
            BuildTaskList(
                TaskCategory.Daily,
                dailyTasksContent
            );
        }
    }


    // =====================================================
    // BADGES
    // =====================================================

    private void RefreshBadges()
    {
        SetBadge(
            achievementsBadge,
            achievementsBadgeText,
            GetClaimableTaskCount(
                TaskCategory.Achievement
            )
        );


        SetBadge(
            dailyTasksBadge,
            dailyTasksBadgeText,
            GetClaimableTaskCount(
                TaskCategory.Daily
            )
        );
    }


    private int GetClaimableTaskCount(
        TaskCategory category
    )
    {
        if (category == TaskCategory.Daily)
        {
            PlayerStats.EnsureDailyReset();
        }


        int count =
            0;


        foreach (
            TaskDefinition task
            in
            taskDatabase.Tasks
        )
        {
            if (task == null)
            {
                continue;
            }


            if (task.Category != category)
            {
                continue;
            }


            if (
                GetTaskProgress(task)
                <
                task.Target
            )
            {
                continue;
            }


            if (
                PlayerStats
                    .IsTaskRewardClaimed(
                        task.Id,
                        task.Category
                    )
            )
            {
                continue;
            }


            count++;
        }


        return count;
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