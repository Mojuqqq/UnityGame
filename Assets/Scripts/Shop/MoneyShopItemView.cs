using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class MoneyShopItemView :
    MonoBehaviour
{
    [Header("UI")]

    [SerializeField]
    private Image icon;


    [SerializeField]
    private TMP_Text titleText;


    [SerializeField]
    private TMP_Text descriptionText;


    [SerializeField]
    private TMP_Text priceText;


    [SerializeField]
    private Button buyButton;


    private MoneyShopProductDefinition
        product;


    private Action<
        MoneyShopProductDefinition
    > buyCallback;


    // =====================================================
    // INITIALIZE
    // =====================================================

    public void Initialize(
        MoneyShopProductDefinition definition,
        string displayedPrice,
        Action<
            MoneyShopProductDefinition
        > onBuy
    )
    {
        product =
            definition;


        buyCallback =
            onBuy;


        if (product == null)
        {
            gameObject.SetActive(
                false
            );

            return;
        }


        if (icon != null)
        {
            icon.sprite =
                product.Icon;


            icon.preserveAspect =
                true;
        }


        if (titleText != null)
        {
            titleText.text =
                product.Title;
        }


        if (descriptionText != null)
        {
            descriptionText.text =
                product.Description;
        }


        if (priceText != null)
        {
            priceText.text =
                displayedPrice;
        }


        if (buyButton != null)
        {
            buyButton.onClick
                .RemoveAllListeners();


            buyButton.onClick
                .AddListener(
                    OnBuyClicked
                );
        }
    }


    private void OnBuyClicked()
    {
        if (product == null)
        {
            return;
        }


        buyCallback?.Invoke(
            product
        );
    }
}