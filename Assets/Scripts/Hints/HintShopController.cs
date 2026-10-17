using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class HintShopController : MonoBehaviour
{
    // =====================================================
    // POPUP
    // =====================================================

    [Header("Popup")]

    [SerializeField]
    private GameObject shopPopup;


    // =====================================================
    // SELECTED HINT
    // =====================================================

    [Header("Selected Hint")]

    [SerializeField]
    private Image selectedHintIcon;

    [SerializeField]
    private TMP_Text ownedText;


    // =====================================================
    // QUANTITY
    // =====================================================

    [Header("Quantity")]

    [SerializeField]
    private Slider quantitySlider;

    [SerializeField]
    private TMP_Text quantityText;

    [SerializeField]
    private TMP_Text costText;

    [SerializeField]
    private Button minusButton;

    [SerializeField]
    private Button plusButton;


    // =====================================================
    // PURCHASE BUTTONS
    // =====================================================

    [Header("Buttons")]

    [SerializeField]
    private Button buyButton;


    // =====================================================
    // MAIN MENU COUNTERS
    // =====================================================

    [Header("Main Menu Counters")]

    [SerializeField]
    private TMP_Text revealPieceCountText;

    [SerializeField]
    private TMP_Text emptyCellsCountText;

    [SerializeField]
    private TMP_Text coinsText;


    // =====================================================
    // ICONS
    // =====================================================

    [Header("Icons")]

    [SerializeField]
    private Sprite revealPieceIcon;

    [SerializeField]
    private Sprite emptyCellsIcon;


    // =====================================================
    // PRICES
    // =====================================================

    [Header("Prices")]

    [SerializeField]
    [Min(1)]
    private int revealPiecePrice =
        50;

    [SerializeField]
    [Min(1)]
    private int emptyCellsPrice =
        30;


    // =====================================================
    // PURCHASE
    // =====================================================

    [Header("Purchase")]

    [SerializeField]
    [Min(1)]
    private int maxPurchaseAmount =
        10;


    // =====================================================
    // REWARDED AD
    // =====================================================

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


    // =====================================================
    // STATE
    // =====================================================

    private HintType selectedType =
        HintType.RevealPiece;


    // =====================================================
    // UNITY
    // =====================================================

    private void Start()
    {
        ConfigureSlider();

        CloseShop();

        RefreshAllUI();
    }


    // =====================================================
    // SLIDER SETUP
    // =====================================================

    private void ConfigureSlider()
    {
        if (quantitySlider == null)
        {
            return;
        }


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


    // =====================================================
    // OPEN SHOP
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
    // QUANTITY
    // =====================================================

    public void OnQuantityChanged(
        float value
    )
    {
        RefreshPopup();
    }


    public void IncreaseQuantity()
    {
        if (quantitySlider == null)
        {
            return;
        }


        quantitySlider.value =
            Mathf.Min(
                quantitySlider.value + 1f,
                quantitySlider.maxValue
            );
    }


    public void DecreaseQuantity()
    {
        if (quantitySlider == null)
        {
            return;
        }


        quantitySlider.value =
            Mathf.Max(
                quantitySlider.value - 1f,
                quantitySlider.minValue
            );
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


        // ---------------------------------------------
        // ICON
        // ---------------------------------------------

        if (selectedHintIcon != null)
        {
            selectedHintIcon.sprite =
                GetHintIcon(
                    selectedType
                );


            selectedHintIcon.preserveAspect =
                true;
        }


        // ---------------------------------------------
        // OWNED
        // ---------------------------------------------

        if (ownedText != null)
        {
            ownedText.text =
                $"В наличии: {owned}";
        }


        // ---------------------------------------------
        // QUANTITY
        // ---------------------------------------------

        if (quantityText != null)
        {
            quantityText.text =
                quantity.ToString();
        }


        // ---------------------------------------------
        // PRICE
        // ---------------------------------------------

        if (costText != null)
        {
            costText.text =
                $"Стоимость: {totalPrice}";
        }


        // ---------------------------------------------
        // MINUS
        // ---------------------------------------------

        if (
            minusButton != null &&
            quantitySlider != null
        )
        {
            minusButton.interactable =
                quantitySlider.value >
                quantitySlider.minValue;
        }


        // ---------------------------------------------
        // PLUS
        // ---------------------------------------------

        if (
            plusButton != null &&
            quantitySlider != null
        )
        {
            plusButton.interactable =
                quantitySlider.value <
                quantitySlider.maxValue;
        }


        // ---------------------------------------------
        // BUY
        // ---------------------------------------------

        if (buyButton != null)
        {
            buyButton.interactable =
                PlayerResources
                    .GetCoins()
                >=
                totalPrice;
        }


        // ---------------------------------------------
        // COINS
        // ---------------------------------------------

        if (coinsText != null)
        {
            coinsText.text =
                PlayerResources
                    .GetCoins()
                    .ToString();
        }
    }


    // =====================================================
    // PRICE
    // =====================================================

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


    // =====================================================
    // ICON
    // =====================================================

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