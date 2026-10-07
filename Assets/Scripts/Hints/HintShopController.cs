using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class HintShopController :
    MonoBehaviour
{
    [Header("Popup")]

    [SerializeField]
    private GameObject shopPopup;


    [Header("Selected Hint")]

    [SerializeField]
    private Image selectedHintIcon;

    [SerializeField]
    private TMP_Text ownedText;


    [Header("Slider")]

    [SerializeField]
    private Slider quantitySlider;

    [SerializeField]
    private TMP_Text quantityText;

    [SerializeField]
    private TMP_Text costText;


    [Header("Buttons")]

    [SerializeField]
    private Button buyButton;


    [Header("Main Menu Counters")]

    [SerializeField]
    private TMP_Text revealPieceCountText;

    [SerializeField]
    private TMP_Text emptyCellsCountText;

    [SerializeField]
    private TMP_Text coinsText;


    [Header("Icons")]

    [SerializeField]
    private Sprite revealPieceIcon;

    [SerializeField]
    private Sprite emptyCellsIcon;


    [Header("Prices")]

    [SerializeField]
    [Min(1)]
    private int revealPiecePrice =
        50;

    [SerializeField]
    [Min(1)]
    private int emptyCellsPrice =
        30;


    [Header("Purchase")]

    [SerializeField]
    [Min(1)]
    private int maxPurchaseAmount =
        10;


    [Header("Rewarded Ad")]

    [SerializeField]
    [Min(1)]
    private int adRewardAmount =
        1;


    [Tooltip(
        "Пока рекламный SDK не подключён, " +
        "кнопка рекламы сразу выдаёт награду."
    )]
    [SerializeField]
    private bool simulateRewardedAd =
        true;


    private HintType selectedType =
        HintType.RevealPiece;


    private void Start()
    {
        if (quantitySlider != null)
        {
            quantitySlider.minValue =
                1;


            quantitySlider.maxValue =
                maxPurchaseAmount;


            quantitySlider.wholeNumbers =
                true;


            quantitySlider.value =
                1;


            quantitySlider.onValueChanged
                .AddListener(
                    OnQuantityChanged
                );
        }


        CloseShop();

        RefreshAllUI();
    }


    // =====================================================
    // OPEN
    // =====================================================

    public void OpenRevealPieceShop()
    {
        OpenShop(
            HintType.RevealPiece
        );
    }


    public void OpenEmptyCellsShop()
    {
        OpenShop(
            HintType.RevealEmptyCells
        );
    }


    private void OpenShop(
        HintType type
    )
    {
        selectedType =
            type;


        if (quantitySlider != null)
        {
            quantitySlider.value =
                1;
        }


        RefreshPopup();


        if (shopPopup != null)
        {
            shopPopup.SetActive(
                true
            );
        }
    }


    public void CloseShop()
    {
        if (shopPopup != null)
        {
            shopPopup.SetActive(
                false
            );
        }
    }


    // =====================================================
    // SLIDER
    // =====================================================

    public void OnQuantityChanged(
        float value
    )
    {
        RefreshPopup();
    }


    private int GetSelectedQuantity()
    {
        if (quantitySlider == null)
        {
            return 1;
        }


        return Mathf.RoundToInt(
            quantitySlider.value
        );
    }


    // =====================================================
    // BUY
    // =====================================================

    public void BuySelectedHints()
    {
        int quantity =
            GetSelectedQuantity();


        int totalPrice =
            GetUnitPrice(
                selectedType
            )
            *
            quantity;


        if (
            !PlayerResources
                .TrySpendCoins(
                    totalPrice
                )
        )
        {
            RefreshPopup();

            return;
        }


        PlayerResources.AddHints(
            selectedType,
            quantity
        );


        RefreshAllUI();
    }


    // =====================================================
    // REWARDED AD
    // =====================================================

    public void WatchAdForHint()
    {
        if (!simulateRewardedAd)
        {
            Debug.LogWarning(
                "Rewarded Ad SDK is not connected yet."
            );

            return;
        }


        // ВРЕМЕННАЯ ЗАГЛУШКА.
        //
        // Когда подключим реальную рекламу,
        // этот вызов должен происходить
        // только после успешного просмотра.
        GrantAdReward();
    }


    public void GrantAdReward()
    {
        PlayerResources.AddHints(
            selectedType,
            adRewardAmount
        );


        RefreshAllUI();
    }


    // =====================================================
    // UI
    // =====================================================

    private void RefreshAllUI()
    {
        if (revealPieceCountText != null)
        {
            revealPieceCountText.text =
                PlayerResources
                    .GetHintCount(
                        HintType.RevealPiece
                    )
                    .ToString();
        }


        if (emptyCellsCountText != null)
        {
            emptyCellsCountText.text =
                PlayerResources
                    .GetHintCount(
                        HintType.RevealEmptyCells
                    )
                    .ToString();
        }


        if (coinsText != null)
        {
            coinsText.text =
                PlayerResources
                    .GetCoins()
                    .ToString();
        }


        RefreshPopup();
    }


    private void RefreshPopup()
    {
        int quantity =
            GetSelectedQuantity();


        int unitPrice =
            GetUnitPrice(
                selectedType
            );


        int totalPrice =
            unitPrice *
            quantity;


        int owned =
            PlayerResources
                .GetHintCount(
                    selectedType
                );


        if (selectedHintIcon != null)
        {
            selectedHintIcon.sprite =
                GetHintIcon(
                    selectedType
                );


            selectedHintIcon
                .preserveAspect =
                true;
        }


        if (ownedText != null)
        {
            ownedText.text =
                $"В наличии: {owned}";
        }


        if (quantityText != null)
        {
            quantityText.text =
                $"Количество: {quantity}";
        }


        if (costText != null)
        {
            costText.text =
                $"Цена: {totalPrice}";
        }


        if (buyButton != null)
        {
            buyButton.interactable =
                PlayerResources
                    .GetCoins()
                >=
                totalPrice;
        }


        if (coinsText != null)
        {
            coinsText.text =
                PlayerResources
                    .GetCoins()
                    .ToString();
        }
    }


    private int GetUnitPrice(
        HintType type
    )
    {
        switch (type)
        {
            case HintType.RevealPiece:

                return
                    revealPiecePrice;


            case HintType.RevealEmptyCells:

                return
                    emptyCellsPrice;
        }


        return 0;
    }


    private Sprite GetHintIcon(
        HintType type
    )
    {
        switch (type)
        {
            case HintType.RevealPiece:

                return
                    revealPieceIcon;


            case HintType.RevealEmptyCells:

                return
                    emptyCellsIcon;
        }


        return null;
    }
}