using System;

namespace Loom.ZombieBattleground.Iap
{
    public enum IapContractType
    {
        ZbgCard,
        FiatPurchase,
        CardFaucet,
        BoosterPack,
        SuperPack,
        AirPack,
        EarthPack,
        FirePack,
        LifePack,
        ToxicPack,
        WaterPack,
        [Obsolete("doesn't actually exist on Marketplace, don't use", true)]
        SmallPack,
        [Obsolete("doesn't actually exist on Marketplace, don't use", true)]
        MinionPack,
        BinancePack,
        TronPack,
        BinancePackFaucet,
        TronPackFaucet,
    }
}
