using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class TaskItemView :
    MonoBehaviour
{
    // =====================================================
    // UI
    // =====================================================

    [Header("Texts")]

    [SerializeField]
    private TMP_Text titleText;

    [SerializeField]
    private TMP_Text progressText;

    [SerializeField]
    private TMP_Text rewardText;


    [Header("Claim Button")]

    [SerializeField]
    private Button claimButton;

    [SerializeField]
    private TMP_Text claimButtonText;


    // =====================================================
    // STATE
    // =====================================================

    private TaskDefinition taskDefinition;

    private Action<TaskDefinition>
        claimCallback;


    // =====================================================
    // INITIALIZE
    // =====================================================

    public void Initialize(
        TaskDefinition definition,
        int currentProgress,
        bool rewardClaimed,
        Action<TaskDefinition> onClaim
    )
    {
        taskDefinition =
            definition;


        claimCallback =
            onClaim;


        if (claimButton != null)
        {
            claimButton.onClick
                .RemoveAllListeners();


            claimButton.onClick
                .AddListener(
                    OnClaimClicked
                );
        }


        Refresh(
            currentProgress,
            rewardClaimed
        );
    }


    // =====================================================
    // REFRESH
    // =====================================================

    public void Refresh(
        int currentProgress,
        bool rewardClaimed
    )
    {
        if (taskDefinition == null)
        {
            return;
        }


        int target =
            taskDefinition.Target;


        int visibleProgress =
            Mathf.Min(
                currentProgress,
                target
            );


        bool completed =
            currentProgress >=
            target;


        // ---------------------------------------------
        // TITLE
        // ---------------------------------------------

        if (titleText != null)
        {
            titleText.text =
                taskDefinition.Title;
        }


        // ---------------------------------------------
        // PROGRESS
        // ---------------------------------------------

        if (progressText != null)
        {
            progressText.text =
                $"{visibleProgress} / {target}";
        }


        // ---------------------------------------------
        // REWARD
        // ---------------------------------------------

        if (rewardText != null)
        {
            rewardText.text =
                $"+{taskDefinition.RewardCoins}";
        }


        // ---------------------------------------------
        // BUTTON
        // ---------------------------------------------

        if (claimButton == null)
        {
            return;
        }


        if (rewardClaimed)
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
    // CLAIM
    // =====================================================

    private void OnClaimClicked()
    {
        if (taskDefinition == null)
        {
            return;
        }


        claimCallback?.Invoke(
            taskDefinition
        );
    }
}