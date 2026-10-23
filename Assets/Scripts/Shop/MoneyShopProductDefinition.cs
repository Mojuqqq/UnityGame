using System;
using UnityEngine;


public enum MoneyShopProductPlacement
{
    Grid,
    RemoveAds
}


public enum MoneyShopStoreProductType
{
    Consumable,
    NonConsumable
}


[Serializable]
public class MoneyShopProductDefinition
{
    // =====================================================
    // ID
    // =====================================================

    [Header("Identity")]

    [SerializeField]
    private string id;


    [SerializeField]
    private string storeProductId;


    // =====================================================
    // DISPLAY
    // =====================================================

    [Header("Display")]

    [SerializeField]
    private string title;


    [SerializeField]
    [TextArea(1, 3)]
    private string description;


    [SerializeField]
    private Sprite icon;


    [Tooltip(
        "Цена только для редактора / тестов. " +
        "Позже настоящую цену лучше получать из магазина."
    )]
    [SerializeField]
    private string fallbackPriceText =
        "$0.99";


    // =====================================================
    // STORE
    // =====================================================

    [Header("Store")]

    [SerializeField]
    private MoneyShopStoreProductType
        storeProductType =
            MoneyShopStoreProductType
                .Consumable;


    [SerializeField]
    private MoneyShopProductPlacement
        placement =
            MoneyShopProductPlacement
                .Grid;


    // =====================================================
    // REWARDS
    // =====================================================

    [Header("Rewards")]

    [SerializeField]
    [Min(0)]
    private int coins;


    [SerializeField]
    [Min(0)]
    private int revealPieceHints;


    [SerializeField]
    [Min(0)]
    private int emptyCellsHints;


    [SerializeField]
    private bool removeAds;


    // =====================================================
    // PUBLIC
    // =====================================================

    public string Id =>
        string.IsNullOrWhiteSpace(id)
            ? ""
            : id.Trim();


    public string StoreProductId =>
        string.IsNullOrWhiteSpace(
            storeProductId
        )
            ? ""
            : storeProductId.Trim();


    public string Title =>
        title;


    public string Description =>
        description;


    public Sprite Icon =>
        icon;


    public string FallbackPriceText =>
        fallbackPriceText;


    public MoneyShopStoreProductType
        StoreProductType =>
            storeProductType;


    public MoneyShopProductPlacement
        Placement =>
            placement;


    public int Coins =>
        Mathf.Max(
            0,
            coins
        );


    public int RevealPieceHints =>
        Mathf.Max(
            0,
            revealPieceHints
        );


    public int EmptyCellsHints =>
        Mathf.Max(
            0,
            emptyCellsHints
        );


    public bool RemoveAds =>
        removeAds;


    // =====================================================
    // VALIDATION
    // =====================================================

    public bool IsValid(
        out string errorMessage
    )
    {
        if (string.IsNullOrWhiteSpace(Id))
        {
            errorMessage =
                "Product ID is empty.";

            return false;
        }


        if (
            string.IsNullOrWhiteSpace(
                StoreProductId
            )
        )
        {
            errorMessage =
                $"Product '{Id}' has no store product ID.";

            return false;
        }


        if (string.IsNullOrWhiteSpace(title))
        {
            errorMessage =
                $"Product '{Id}' has no title.";

            return false;
        }


        bool hasReward =
            Coins > 0 ||
            RevealPieceHints > 0 ||
            EmptyCellsHints > 0 ||
            RemoveAds;


        if (!hasReward)
        {
            errorMessage =
                $"Product '{Id}' has no reward.";

            return false;
        }


        if (
            RemoveAds &&
            StoreProductType !=
            MoneyShopStoreProductType
                .NonConsumable
        )
        {
            errorMessage =
                $"Product '{Id}' removes ads " +
                "but is not NonConsumable.";

            return false;
        }


        errorMessage =
            "";

        return true;
    }
}