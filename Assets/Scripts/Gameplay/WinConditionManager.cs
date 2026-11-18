using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class WinConditionManager :
    MonoBehaviour
{
    // =====================================================
    // GAMEPLAY
    // =====================================================

    [Header("Gameplay")]

    [SerializeField]
    private GridManager gridManager;


    [SerializeField]
    private HintManager hintManager;


    [SerializeField]
    private LevelManager levelManager;


    // =====================================================
    // COMPLETE UI
    // =====================================================

    [Header("Complete UI")]

    [SerializeField]
    private GameObject levelCompleteOverlay;


    [SerializeField]
    private TMP_Text completeTitle;


    [SerializeField]
    private TMP_Text rewardText;


    [SerializeField]
    private Button continueButton;


    [SerializeField]
    private TMP_Text continueButtonText;


    [SerializeField]
    private Button tripleRewardButton;


    [SerializeField]
    private TMP_Text tripleRewardButtonText;


    // =====================================================
    // REWARDED AD
    // =====================================================

    [Header("Rewarded Ad")]

    [Tooltip(
        "Пока настоящий рекламный SDK " +
        "не подключён, кнопка сразу " +
        "считает рекламу просмотренной."
    )]
    [SerializeField]
    private bool simulateRewardedAd =
        true;


    // =====================================================
    // STATE
    // =====================================================

    private bool levelCompleted;


    private int currentDisplayedReward;


    // =====================================================
    // EVENTS
    // =====================================================

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


    // =====================================================
    // START
    // =====================================================

    private void Start()
    {
        levelCompleted =
            false;


        currentDisplayedReward =
            0;


        if (levelCompleteOverlay != null)
        {
            levelCompleteOverlay
                .SetActive(
                    false
                );
        }
    }


    // =====================================================
    // WIN CHECK
    // =====================================================

    private void CheckWinCondition()
    {
        if (levelCompleted)
        {
            return;
        }


        if (hintManager == null)
        {
            Debug.LogError(
                "WinConditionManager: " +
                "HintManager is not assigned."
            );

            return;
        }


        if (!hintManager.IsSolved())
        {
            return;
        }


        CompleteLevel();
    }


    // =====================================================
    // COMPLETE
    // =====================================================

    private void CompleteLevel()
    {
        levelCompleted =
            true;


        if (levelManager == null)
        {
            Debug.LogError(
                "WinConditionManager: " +
                "LevelManager is not assigned."
            );

            return;
        }


        currentDisplayedReward =
            levelManager
                .CompleteCurrentLevel();


        RefreshCompleteUI();


        if (levelCompleteOverlay != null)
        {
            levelCompleteOverlay
                .SetActive(
                    true
                );
        }


        Debug.Log(
            "Level completed!"
        );
    }


    // =====================================================
    // UI
    // =====================================================

    private void RefreshCompleteUI()
    {
        if (levelManager == null)
        {
            return;
        }


        bool hasNextLevel =
            levelManager.HasNextLevel();


        // =================================================
        // TITLE
        // =================================================

        if (completeTitle != null)
        {
            completeTitle.text =
                hasNextLevel
                    ? "УРОВЕНЬ ПРОЙДЕН!"
                    : "ВСЕ УРОВНИ ПРОЙДЕНЫ!";
        }


        // =================================================
        // REWARD
        // =================================================

        if (rewardText != null)
        {
            if (currentDisplayedReward > 0)
            {
                rewardText.text =
                    $"+{currentDisplayedReward} МОНЕТ";
            }
            else
            {
                rewardText.text =
                    "НАГРАДА УЖЕ ПОЛУЧЕНА";
            }
        }


        // =================================================
        // CONTINUE
        // =================================================

        if (continueButton != null)
        {
            continueButton.interactable =
                true;
        }


        if (continueButtonText != null)
        {
            continueButtonText.text =
                hasNextLevel
                    ? "ПРОДОЛЖИТЬ"
                    : "В МЕНЮ";
        }


        RefreshTripleRewardButton();
    }


    // =====================================================
    // TRIPLE REWARD
    // =====================================================

    private void RefreshTripleRewardButton()
    {
        if (
            levelManager == null ||
            tripleRewardButton == null
        )
        {
            return;
        }


        bool canClaim =
            levelManager
                .CanClaimTripleReward();


        tripleRewardButton.interactable =
            canClaim;


        if (tripleRewardButtonText == null)
        {
            return;
        }


        if (canClaim)
        {
            tripleRewardButtonText.text =
                "x3 ЗА РЕКЛАМУ";
        }
        else if (
            currentDisplayedReward >=
            levelManager.TripleRewardTotalCoins
        )
        {
            tripleRewardButtonText.text =
                "ПОЛУЧЕНО";
        }
        else
        {
            tripleRewardButtonText.text =
                "НЕДОСТУПНО";
        }
    }


    // =====================================================
    // CONTINUE BUTTON
    // =====================================================

    public void Continue()
    {
        if (levelManager == null)
        {
            return;
        }


        levelManager
            .ContinueAfterCompletion();
    }


    // =====================================================
    // REWARDED AD BUTTON
    // =====================================================

    public void WatchAdForTripleReward()
    {
        if (
            levelManager == null ||
            !levelManager
                .CanClaimTripleReward()
        )
        {
            return;
        }


        if (simulateRewardedAd)
        {
            GrantTripleRewardAfterAd();

            return;
        }


        Debug.LogWarning(
            "Rewarded Ad SDK is not connected yet."
        );
    }


    public void GrantTripleRewardAfterAd()
    {
        if (levelManager == null)
        {
            return;
        }


        bool granted =
            levelManager
                .TryGrantTripleRewardBonus();


        if (!granted)
        {
            return;
        }


        currentDisplayedReward =
            levelManager
                .TripleRewardTotalCoins;


        if (rewardText != null)
        {
            rewardText.text =
                $"+{currentDisplayedReward} МОНЕТ";
        }


        RefreshTripleRewardButton();
    }
}