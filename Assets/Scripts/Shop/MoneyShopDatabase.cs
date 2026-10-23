using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(
    fileName = "MoneyShopDatabase",
    menuName = "Puzzle/Shop/Money Shop Database"
)]
public class MoneyShopDatabase :
    ScriptableObject
{
    [SerializeField]
    private List<MoneyShopProductDefinition>
        products =
            new List<
                MoneyShopProductDefinition
            >();


    public IReadOnlyList<
        MoneyShopProductDefinition
    > Products =>
        products;


    // =====================================================
    // FIND
    // =====================================================

    public MoneyShopProductDefinition
        GetByStoreProductId(
            string storeProductId
        )
    {
        if (
            string.IsNullOrWhiteSpace(
                storeProductId
            )
        )
        {
            return null;
        }


        foreach (
            MoneyShopProductDefinition product
            in products
        )
        {
            if (product == null)
            {
                continue;
            }


            if (
                product.StoreProductId ==
                storeProductId
            )
            {
                return product;
            }
        }


        return null;
    }


    // =====================================================
    // VALIDATION
    // =====================================================

    public bool IsValid(
        out string errorMessage
    )
    {
        HashSet<string> ids =
            new HashSet<string>();


        HashSet<string> storeIds =
            new HashSet<string>();


        for (
            int i = 0;
            i < products.Count;
            i++
        )
        {
            MoneyShopProductDefinition product =
                products[i];


            if (product == null)
            {
                errorMessage =
                    $"Product {i} is null.";

                return false;
            }


            if (
                !product.IsValid(
                    out string productError
                )
            )
            {
                errorMessage =
                    $"Product {i}: " +
                    productError;

                return false;
            }


            if (!ids.Add(product.Id))
            {
                errorMessage =
                    $"Duplicate product ID: " +
                    product.Id;

                return false;
            }


            if (
                !storeIds.Add(
                    product.StoreProductId
                )
            )
            {
                errorMessage =
                    $"Duplicate store product ID: " +
                    product.StoreProductId;

                return false;
            }
        }


        errorMessage =
            "";

        return true;
    }
}