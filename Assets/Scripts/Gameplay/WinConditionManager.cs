using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WinConditionManager : MonoBehaviour
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

    [SerializeField]
    private PieceSpawner pieceSpawner;


    [Header("Gameplay Input")]

    [SerializeField]
    private CanvasGroup[] gameplayInputGroups;


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
    // VICTORY ANIMATION
    // =====================================================

    [Header("Victory Animation")]

    [SerializeField]
    private Image victoryFlash;

    [SerializeField]
    [Min(0f)]
    private float victoryDelay = 0.20f;

    [SerializeField]
    [Min(0.01f)]
    private float flashDuration = 0.25f;

    [SerializeField]
    [Range(0f, 1f)]
    private float flashMaxAlpha = 0.20f;

    [SerializeField]
    [Min(0.01f)]
    private float rewardCountDuration = 0.65f;


    // =====================================================
    // ADS
    // =====================================================

    [Header("Rewarded Ad")]

    [SerializeField]
    private bool simulateRewardedAd = true;

    // Future AdsManager integration.
    // Callback arguments: adShown, rewardEarned.
    public event Action<Action<bool, bool>>
        RewardedAdRequested;

    // Callback must be invoked after ad close
    // or if the ad is unavailable.
    public event Action<Action>
        InterstitialAdRequested;


    // =====================================================
    // STATE
    // =====================================================

    private PopupAnimator popupAnimator;

    private bool levelCompleted;
    private bool rewardAnimating;
    private bool rewardPending;
    private bool rewardedShown;
    private bool leaving;
    private bool menuLoaded;
    private bool victoryReady;

    private int currentDisplayedReward;

    private bool CanSimulateRewarded =>
        simulateRewardedAd &&
        (Application.isEditor || Debug.isDebugBuild);


    // =====================================================
    // UNITY
    // =====================================================

    private void Awake()
    {
        if (levelCompleteOverlay != null)
        {
            popupAnimator =
                levelCompleteOverlay
                    .GetComponent<PopupAnimator>();

            if (popupAnimator != null)
                popupAnimator.HideImmediate();
            else
                levelCompleteOverlay.SetActive(false);
        }

        if (victoryFlash != null)
            victoryFlash.gameObject.SetActive(false);
    }


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
    // WIN CHECK
    // =====================================================

    private void CheckWinCondition()
    {
        if (
            levelCompleted ||
            hintManager == null ||
            levelManager == null ||
            levelManager.CurrentLevel == null
        )
        {
            return;
        }

        if (!hintManager.IsSolved())
            return;

        levelCompleted = true;

        LockGameplayInput();

        StartCoroutine(VictorySequence());
    }


    private void LockGameplayInput()
    {
        if (pieceSpawner != null)
            pieceSpawner.LockGameplay();

        if (gameplayInputGroups == null)
            return;

        foreach (CanvasGroup group in gameplayInputGroups)
        {
            if (group == null)
                continue;

            group.interactable = false;
            group.blocksRaycasts = false;
        }
    }


    // =====================================================
    // VICTORY SEQUENCE
    // =====================================================

    private IEnumerator VictorySequence()
    {
        // GridChanged вызывается внутри PlacePiece().
        // Даём PieceDragHandler завершить фиксацию.
        yield return null;

        currentDisplayedReward =
            levelManager.CompleteCurrentLevel();

        if (victoryDelay > 0f)
        {
            yield return new WaitForSecondsRealtime(
                victoryDelay
            );
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySfx(
                GameSfx.LevelComplete
            );
        }

        HapticManager.Vibrate();

        yield return PlayVictoryFlash();

        PrepareCompleteUI();

        if (levelCompleteOverlay != null)
        {
            levelCompleteOverlay.transform
                .SetAsLastSibling();

            if (popupAnimator != null)
                popupAnimator.Open();
            else
                levelCompleteOverlay.SetActive(true);
        }

        yield return AnimateReward(
            0,
            currentDisplayedReward
        );

        victoryReady = true;

        RefreshButtons();
    }


    // =====================================================
    // VICTORY FLASH
    // =====================================================

    private IEnumerator PlayVictoryFlash()
    {
        if (victoryFlash == null)
            yield break;

        Color color = victoryFlash.color;

        color.a = 0f;

        victoryFlash.color = color;

        victoryFlash.raycastTarget = false;

        victoryFlash.gameObject.SetActive(true);

        float elapsed = 0f;

        float duration = Mathf.Max(
            0.01f,
            flashDuration
        );

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                elapsed / duration
            );

            color.a =
                Mathf.Sin(t * Mathf.PI) *
                flashMaxAlpha;

            victoryFlash.color = color;

            yield return null;
        }

        color.a = 0f;

        victoryFlash.color = color;

        victoryFlash.gameObject.SetActive(false);
    }


    // =====================================================
    // COMPLETE UI
    // =====================================================

    private void PrepareCompleteUI()
    {
        if (completeTitle != null)
        {
            completeTitle.text =
                levelManager.HasNextLevel()
                    ? "УРОВЕНЬ ПРОЙДЕН!"
                    : "ВСЕ УРОВНИ ПРОЙДЕНЫ!";
        }

        if (continueButtonText != null)
        {
            continueButtonText.text = "В МЕНЮ";
        }

        SetRewardText(0, currentDisplayedReward == 0);

        RefreshButtons();
    }


    private void SetRewardText(
        int amount,
        bool alreadyClaimed = false
    )
    {
        if (rewardText == null)
            return;

        rewardText.text = alreadyClaimed
            ? "НАГРАДА УЖЕ ПОЛУЧЕНА"
            : $"+{amount} ЗВЁЗД";
    }


    private void RefreshButtons()
    {
        bool ready =
            victoryReady &&
            !rewardAnimating &&
            !rewardPending &&
            !leaving;

        bool canClaim =
            levelManager != null &&
            levelManager.CanClaimTripleReward();

        bool adConnected =
            CanSimulateRewarded ||
            RewardedAdRequested != null;

        if (continueButton != null)
        {
            continueButton.interactable = ready;
        }

        if (tripleRewardButton != null)
        {
            tripleRewardButton.interactable =
                ready &&
                canClaim &&
                adConnected;
        }

        if (tripleRewardButtonText == null)
            return;

        if (rewardPending)
        {
            tripleRewardButtonText.text =
                "ЗАГРУЗКА...";
        }
        else if (
            currentDisplayedReward > 0 &&
            currentDisplayedReward >=
                levelManager.TripleRewardTotalCoins &&
            !canClaim
        )
        {
            tripleRewardButtonText.text =
                "ПОЛУЧЕНО";
        }
        else if (canClaim && adConnected)
        {
            tripleRewardButtonText.text =
                "×3 ЗА РЕКЛАМУ";
        }
        else
        {
            tripleRewardButtonText.text =
                "НЕДОСТУПНО";
        }
    }


    // =====================================================
    // REWARD ANIMATION
    // =====================================================

    private IEnumerator AnimateReward(
        int from,
        int to
    )
    {
        rewardAnimating = true;

        RefreshButtons();

        if (to <= 0)
        {
            SetRewardText(0, true);
        }
        else
        {
            float elapsed = 0f;

            float duration = Mathf.Max(
                0.01f,
                rewardCountDuration
            );

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;

                float t = Mathf.Clamp01(
                    elapsed / duration
                );

                float eased =
                    1f - Mathf.Pow(1f - t, 3f);

                int amount = Mathf.RoundToInt(
                    Mathf.Lerp(from, to, eased)
                );

                SetRewardText(amount);

                yield return null;
            }

            SetRewardText(to);
        }

        rewardAnimating = false;

        RefreshButtons();
    }


    // =====================================================
    // REWARDED AD
    // =====================================================

    public void WatchAdForTripleReward()
    {
        if (
            !victoryReady ||
            rewardAnimating ||
            rewardPending ||
            leaving ||
            levelManager == null ||
            !levelManager.CanClaimTripleReward()
        )
        {
            return;
        }

        rewardPending = true;

        RefreshButtons();

        if (RewardedAdRequested != null)
        {
            try
            {
                RewardedAdRequested.Invoke(
                    OnRewardedAdFinished
                );
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);

                OnRewardedAdFinished(false, false);
            }

            return;
        }

        if (CanSimulateRewarded)
        {
            OnRewardedAdFinished(true, true);

            return;
        }

        OnRewardedAdFinished(false, false);
    }


    private void OnRewardedAdFinished(
        bool adShown,
        bool rewardEarned
    )
    {
        if (this == null || !rewardPending)
            return;

        rewardPending = false;

        rewardedShown |= adShown;

        if (!adShown || !rewardEarned)
        {
            RefreshButtons();
            return;
        }

        if (!levelManager.TryGrantTripleRewardBonus())
        {
            RefreshButtons();
            return;
        }

        int previousReward = currentDisplayedReward;

        currentDisplayedReward =
            levelManager.TripleRewardTotalCoins;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySfx(
                GameSfx.Reward
            );
        }

        StartCoroutine(
            AnimateReward(
                previousReward,
                currentDisplayedReward
            )
        );
    }


    public void GrantTripleRewardAfterAd()
    {
        OnRewardedAdFinished(true, true);
    }


    // =====================================================
    // CONTINUE / INTERSTITIAL
    // =====================================================

    public void Continue()
    {
        if (
            !victoryReady ||
            rewardPending ||
            rewardAnimating ||
            leaving ||
            levelManager == null
        )
        {
            return;
        }

        leaving = true;

        RefreshButtons();

        StartCoroutine(ExitSequence());
    }


    private IEnumerator ExitSequence()
    {
        if (popupAnimator != null)
        {
            popupAnimator.Close();

            while (levelCompleteOverlay.activeSelf)
                yield return null;
        }

        bool canShowInterstitial =
            !rewardedShown &&
            !PlayerEntitlements.AreAdsRemoved() &&
            InterstitialAdRequested != null;

        if (!canShowInterstitial)
        {
            FinishExit();
            yield break;
        }

        try
        {
            InterstitialAdRequested.Invoke(
                FinishExit
            );
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);

            FinishExit();
        }
    }


    private void FinishExit()
    {
        if (this == null || menuLoaded)
            return;

        menuLoaded = true;

        levelManager.ContinueAfterCompletion();
    }
}