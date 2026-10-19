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
    // CURRENT LEVEL
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
    // ACHIEVEMENTS POPUP
    // =====================================================

    [Header("Achievements Popup")]

    [SerializeField]
    private GameObject achievementsPopup;


    // Achievement 1

    [SerializeField]
    private TMP_Text achievement1ProgressText;

    [SerializeField]
    private TMP_Text achievement1RewardText;

    [SerializeField]
    private Button achievement1ClaimButton;

    [SerializeField]
    private TMP_Text achievement1ClaimButtonText;


    // Achievement 2

    [SerializeField]
    private TMP_Text achievement2ProgressText;

    [SerializeField]
    private TMP_Text achievement2RewardText;

    [SerializeField]
    private Button achievement2ClaimButton;

    [SerializeField]
    private TMP_Text achievement2ClaimButtonText;


    // Achievement 3

    [SerializeField]
    private TMP_Text achievement3ProgressText;

    [SerializeField]
    private TMP_Text achievement3RewardText;

    [SerializeField]
    private Button achievement3ClaimButton;

    [SerializeField]
    private TMP_Text achievement3ClaimButtonText;


    [SerializeField]
    private GameObject achievementsBadge;

    [SerializeField]
    private TMP_Text achievementsBadgeText;


    // =====================================================
    // DAILY TASKS POPUP
    // =====================================================

    [Header("Daily Tasks Popup")]

    [SerializeField]
    private GameObject dailyTasksPopup;


    // Daily 1

    [SerializeField]
    private TMP_Text daily1ProgressText;

    [SerializeField]
    private TMP_Text daily1RewardText;

    [SerializeField]
    private Button daily1ClaimButton;

    [SerializeField]
    private TMP_Text daily1ClaimButtonText;


    // Daily 2

    [SerializeField]
    private TMP_Text daily2ProgressText;

    [SerializeField]
    private TMP_Text daily2RewardText;

    [SerializeField]
    private Button daily2ClaimButton;

    [SerializeField]
    private TMP_Text daily2ClaimButtonText;


    // Daily 3

    [SerializeField]
    private TMP_Text daily3ProgressText;

    [SerializeField]
    private TMP_Text daily3RewardText;

    [SerializeField]
    private Button daily3ClaimButton;

    [SerializeField]
    private TMP_Text daily3ClaimButtonText;


    [SerializeField]
    private GameObject dailyTasksBadge;

    [SerializeField]
    private TMP_Text dailyTasksBadgeText;


    // =====================================================
    // REWARDS
    // =====================================================

    [Header("Daily Rewards")]

    [SerializeField]
    [Min(0)]
    private int dailyReward1 =
        25;

    [SerializeField]
    [Min(0)]
    private int dailyReward2 =
        50;

    [SerializeField]
    [Min(0)]
    private int dailyReward3 =
        25;


    [Header("Achievement Rewards")]

    [SerializeField]
    [Min(0)]
    private int achievementReward1 =
        100;

    [SerializeField]
    [Min(0)]
    private int achievementReward2 =
        150;

    [SerializeField]
    [Min(0)]
    private int achievementReward3 =
        100;


    // =====================================================
    // SCENE
    // =====================================================

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


        RefreshRewardTask(
            piecesPlaced,
            5,
            achievementReward1,
            PlayerStats
                .IsAchievementRewardClaimed(0),
            achievement1ProgressText,
            achievement1RewardText,
            achievement1ClaimButton,
            achievement1ClaimButtonText
        );


        RefreshRewardTask(
            levelsCompleted,
            3,
            achievementReward2,
            PlayerStats
                .IsAchievementRewardClaimed(1),
            achievement2ProgressText,
            achievement2RewardText,
            achievement2ClaimButton,
            achievement2ClaimButtonText
        );


        RefreshRewardTask(
            hintsUsed,
            3,
            achievementReward3,
            PlayerStats
                .IsAchievementRewardClaimed(2),
            achievement3ProgressText,
            achievement3RewardText,
            achievement3ClaimButton,
            achievement3ClaimButtonText
        );
    }


    // =====================================================
    // ACHIEVEMENT CLAIM
    // =====================================================

    public void ClaimAchievement1()
    {
        TryClaimAchievement(
            0,
            PlayerStats.GetTotalPiecesPlaced(),
            5,
            achievementReward1
        );
    }


    public void ClaimAchievement2()
    {
        TryClaimAchievement(
            1,
            ProgressManager
                .GetCompletedLevelCount(
                    levelDatabase
                ),
            3,
            achievementReward2
        );
    }


    public void ClaimAchievement3()
    {
        TryClaimAchievement(
            2,
            PlayerStats.GetTotalHintsUsed(),
            3,
            achievementReward3
        );
    }


    private void TryClaimAchievement(
        int index,
        int current,
        int target,
        int reward
    )
    {
        if (current < target)
        {
            return;
        }


        if (
            PlayerStats
                .IsAchievementRewardClaimed(
                    index
                )
        )
        {
            return;
        }


        PlayerStats
            .MarkAchievementRewardClaimed(
                index
            );


        PlayerResources.AddCoins(
            reward
        );


        RefreshMenu();
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
        PlayerStats.EnsureDailyReset();


        int levels =
            PlayerStats
                .GetDailyLevelsCompleted();


        int hints =
            PlayerStats
                .GetDailyHintsUsed();


        RefreshRewardTask(
            levels,
            1,
            dailyReward1,
            PlayerStats
                .IsDailyRewardClaimed(0),
            daily1ProgressText,
            daily1RewardText,
            daily1ClaimButton,
            daily1ClaimButtonText
        );


        RefreshRewardTask(
            levels,
            3,
            dailyReward2,
            PlayerStats
                .IsDailyRewardClaimed(1),
            daily2ProgressText,
            daily2RewardText,
            daily2ClaimButton,
            daily2ClaimButtonText
        );


        RefreshRewardTask(
            hints,
            1,
            dailyReward3,
            PlayerStats
                .IsDailyRewardClaimed(2),
            daily3ProgressText,
            daily3RewardText,
            daily3ClaimButton,
            daily3ClaimButtonText
        );
    }


    // =====================================================
    // DAILY CLAIM
    // =====================================================

    public void ClaimDaily1()
    {
        TryClaimDaily(
            0,
            PlayerStats
                .GetDailyLevelsCompleted(),
            1,
            dailyReward1
        );
    }


    public void ClaimDaily2()
    {
        TryClaimDaily(
            1,
            PlayerStats
                .GetDailyLevelsCompleted(),
            3,
            dailyReward2
        );
    }


    public void ClaimDaily3()
    {
        TryClaimDaily(
            2,
            PlayerStats
                .GetDailyHintsUsed(),
            1,
            dailyReward3
        );
    }


    private void TryClaimDaily(
        int index,
        int current,
        int target,
        int reward
    )
    {
        PlayerStats.EnsureDailyReset();


        if (current < target)
        {
            return;
        }


        if (
            PlayerStats
                .IsDailyRewardClaimed(
                    index
                )
        )
        {
            return;
        }


        PlayerStats
            .MarkDailyRewardClaimed(
                index
            );


        PlayerResources.AddCoins(
            reward
        );


        RefreshMenu();
    }


    // =====================================================
    // TASK UI
    // =====================================================

    private void RefreshRewardTask(
        int current,
        int target,
        int reward,
        bool claimed,
        TMP_Text progress,
        TMP_Text rewardText,
        Button claimButton,
        TMP_Text claimButtonText
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


        if (progress != null)
        {
            progress.text =
                $"{visibleCurrent} / {target}";
        }


        if (rewardText != null)
        {
            rewardText.text =
                $"+{reward}";
        }


        if (claimButton == null)
        {
            return;
        }


        if (claimed)
        {
            claimButton.interactable =
                false;


            if (claimButtonText != null)
            {
                claimButtonText.text =
                    "ПОЛУЧЕНО";
            }


            return;
        }


        if (completed)
        {
            claimButton.interactable =
                true;


            if (claimButtonText != null)
            {
                claimButtonText.text =
                    "ПОЛУЧИТЬ";
            }


            return;
        }


        claimButton.interactable =
            false;


        if (claimButtonText != null)
        {
            claimButtonText.text =
                "НЕ ГОТОВО";
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
            GetClaimableAchievementCount()
        );


        SetBadge(
            dailyTasksBadge,
            dailyTasksBadgeText,
            GetClaimableDailyRewardCount()
        );
    }


    private int GetClaimableAchievementCount()
    {
        int count =
            0;


        if (
            PlayerStats.GetTotalPiecesPlaced()
            >=
            5
            &&
            !PlayerStats
                .IsAchievementRewardClaimed(0)
        )
        {
            count++;
        }


        if (
            ProgressManager
                .GetCompletedLevelCount(
                    levelDatabase
                )
            >=
            3
            &&
            !PlayerStats
                .IsAchievementRewardClaimed(1)
        )
        {
            count++;
        }


        if (
            PlayerStats.GetTotalHintsUsed()
            >=
            3
            &&
            !PlayerStats
                .IsAchievementRewardClaimed(2)
        )
        {
            count++;
        }


        return count;
    }


    private int GetClaimableDailyRewardCount()
    {
        PlayerStats.EnsureDailyReset();


        int count =
            0;


        int levels =
            PlayerStats
                .GetDailyLevelsCompleted();


        int hints =
            PlayerStats
                .GetDailyHintsUsed();


        if (
            levels >= 1
            &&
            !PlayerStats
                .IsDailyRewardClaimed(0)
        )
        {
            count++;
        }


        if (
            levels >= 3
            &&
            !PlayerStats
                .IsDailyRewardClaimed(1)
        )
        {
            count++;
        }


        if (
            hints >= 1
            &&
            !PlayerStats
                .IsDailyRewardClaimed(2)
        )
        {
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