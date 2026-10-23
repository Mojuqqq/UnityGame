using System;


public static class MoneyShopRewardService
{
    public static event Action RewardsChanged;


    public static void Grant(
        MoneyShopProductDefinition product
    )
    {
        if (product == null)
        {
            return;
        }


        if (product.Coins > 0)
        {
            PlayerResources.AddCoins(
                product.Coins
            );
        }


        if (product.RevealPieceHints > 0)
        {
            PlayerResources.AddHints(
                HintType.RevealPiece,
                product.RevealPieceHints
            );
        }


        if (product.EmptyCellsHints > 0)
        {
            PlayerResources.AddHints(
                HintType.RevealEmptyCells,
                product.EmptyCellsHints
            );
        }


        if (product.RemoveAds)
        {
            PlayerEntitlements.SetAdsRemoved(
                true
            );
        }


        RewardsChanged?.Invoke();
    }
}