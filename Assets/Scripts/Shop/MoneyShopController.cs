using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


[Serializable]
public class StringUnityEvent :
    UnityEvent<string>
{
}


public class MoneyShopController :
    MonoBehaviour
{
    // =====================================================
    // POPUP
    // =====================================================

    [Header("Popup")]

    [SerializeField]
    private GameObject shopPopup;


    // =====================================================
    // PRODUCTS
    // =====================================================

    [Header("Products")]

    [SerializeField]
    private MoneyShopDatabase database;

    [SerializeField]
    private MoneyShopItemView itemPrefab;

    [SerializeField]
    private RectTransform productsContent;


    // =====================================================
    // REMOVE ADS
    // =====================================================

    [Header("Remove Ads")]

    [SerializeField]
    private Button removeAdsButton;

    [SerializeField]
    private TMP_Text removeAdsButtonText;

    [SerializeField]
    private TMP_Text removeAdsPriceText;


    // =====================================================
    // COUNTERS
    // =====================================================

    [Header("Counters")]

    [SerializeField]
    private TMP_Text coinsText;

    [SerializeField]
    private TMP_Text revealPieceCountText;

    [SerializeField]
    private TMP_Text emptyCellsCountText;


    // =====================================================
    // TESTING
    // =====================================================

    [Header("Testing")]

    [Tooltip(
        "ON = покупка сразу выдаётся без настоящего магазина."
    )]
    [SerializeField]
    private bool simulatePurchases =
        true;


    // =====================================================
    // REAL PURCHASE
    // =====================================================

    [Header("Real Purchase")]

    [SerializeField]
    private StringUnityEvent
        purchaseRequested;


    // =====================================================
    // STATE
    // =====================================================

    private MoneyShopProductDefinition
        removeAdsProduct;


    // =====================================================
    // UNITY
    // =====================================================

    private void OnEnable()
    {
        MoneyShopRewardService
            .RewardsChanged +=
                OnRewardsChanged;
    }


    private void OnDisable()
    {
        MoneyShopRewardService
            .RewardsChanged -=
                OnRewardsChanged;
    }


    private void Start()
    {
        CloseShop();

        RefreshCounters();
    }


    // =====================================================
    // OPEN
    // =====================================================

    public void OpenShop()
    {
        if (!ValidateReferences())
        {
            return;
        }


        // Сначала включаем popup,
        // чтобы Unity Layout работал
        // уже на активных объектах.
        shopPopup.SetActive(
            true
        );


        BuildProducts();

        RefreshCounters();

        RefreshRemoveAdsButton();


        Canvas.ForceUpdateCanvases();


        LayoutRebuilder
            .ForceRebuildLayoutImmediate(
                productsContent
            );


        Debug.Log(
            $"MoneyShop: built " +
            $"{productsContent.childCount} " +
            $"product cards."
        );
    }


    // =====================================================
    // CLOSE
    // =====================================================

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
    // VALIDATION
    // =====================================================

    private bool ValidateReferences()
    {
        if (shopPopup == null)
        {
            Debug.LogError(
                "MoneyShopController: " +
                "Shop Popup is not assigned."
            );

            return false;
        }


        if (database == null)
        {
            Debug.LogError(
                "MoneyShopController: " +
                "Database is not assigned."
            );

            return false;
        }


        if (
            !database.IsValid(
                out string errorMessage
            )
        )
        {
            Debug.LogError(
                "MoneyShopController: " +
                errorMessage
            );

            return false;
        }


        if (itemPrefab == null)
        {
            Debug.LogError(
                "MoneyShopController: " +
                "Item Prefab is not assigned."
            );

            return false;
        }


        if (productsContent == null)
        {
            Debug.LogError(
                "MoneyShopController: " +
                "Products Content is not assigned."
            );

            return false;
        }


        return true;
    }


    // =====================================================
    // BUILD PRODUCTS
    // =====================================================

    private void BuildProducts()
    {
        ClearProducts();


        removeAdsProduct =
            null;


        int builtCount =
            0;


        foreach (
            MoneyShopProductDefinition product
            in database.Products
        )
        {
            if (product == null)
            {
                continue;
            }


            if (
                product.Placement ==
                MoneyShopProductPlacement
                    .RemoveAds
            )
            {
                removeAdsProduct =
                    product;

                continue;
            }


            MoneyShopItemView item =
                Instantiate(
                    itemPrefab,
                    productsContent
                );


            item.Initialize(
                product,
                product.FallbackPriceText,
                RequestPurchase
            );


            builtCount++;
        }


        Debug.Log(
            $"MoneyShop: created " +
            $"{builtCount} grid products."
        );
    }


    // =====================================================
    // CLEAR PRODUCTS
    // =====================================================

    private void ClearProducts()
    {
        for (
            int i =
                productsContent.childCount - 1;

            i >= 0;

            i--
        )
        {
            GameObject child =
                productsContent
                    .GetChild(i)
                    .gameObject;


            child.SetActive(
                false
            );


            Destroy(
                child
            );
        }
    }


    // =====================================================
    // PURCHASE
    // =====================================================

    private void RequestPurchase(
        MoneyShopProductDefinition product
    )
    {
        if (product == null)
        {
            return;
        }


        if (simulatePurchases)
        {
            Debug.Log(
                $"SIMULATED PURCHASE: " +
                $"{product.StoreProductId}"
            );


            MoneyShopRewardService
                .Grant(
                    product
                );


            return;
        }


        purchaseRequested?.Invoke(
            product.StoreProductId
        );
    }


    // =====================================================
    // REMOVE ADS
    // =====================================================

    public void BuyRemoveAds()
    {
        if (removeAdsProduct == null)
        {
            return;
        }


        if (
            PlayerEntitlements
                .AreAdsRemoved()
        )
        {
            return;
        }


        RequestPurchase(
            removeAdsProduct
        );
    }


    private void RefreshRemoveAdsButton()
    {
        if (removeAdsButton == null)
        {
            return;
        }


        if (removeAdsProduct == null)
        {
            removeAdsButton
                .gameObject
                .SetActive(
                    false
                );

            return;
        }


        removeAdsButton
            .gameObject
            .SetActive(
                true
            );


        bool owned =
            PlayerEntitlements
                .AreAdsRemoved();


        removeAdsButton.interactable =
            !owned;


        if (removeAdsButtonText != null)
        {
            removeAdsButtonText.text =
                owned
                    ? "РЕКЛАМА ОТКЛЮЧЕНА"
                    : removeAdsProduct.Title;
        }


        if (removeAdsPriceText != null)
        {
            removeAdsPriceText.text =
                owned
                    ? ""
                    : removeAdsProduct
                        .FallbackPriceText;
        }
    }


    // =====================================================
    // COUNTERS
    // =====================================================

    private void RefreshCounters()
    {
        if (coinsText != null)
        {
            coinsText.text =
                PlayerResources
                    .GetCoins()
                    .ToString();
        }


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
    }


    private void OnRewardsChanged()
    {
        RefreshCounters();

        RefreshRemoveAdsButton();
    }
}