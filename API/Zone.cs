using System;

namespace Cardgame.API
{
    public enum Zone
    {
        Create,
        CountableDeck,
        DeckTop1,
        DeckTop2,
        DeckTop3,
        DeckTop4,
        Discard,
        Hand,
        InPlay,
        PlayerMat,
        RecentGains,
        Stash,
        SupplyAll,
        SupplyAvailable,
        SupplyEmpty,
        Trash,        
    }

    public static class ZoneExtensions
    {
        public static bool IsPrivate(this Zone zone)
        {
            return zone switch 
            {
                Zone.CountableDeck => true,
                Zone.Create => true,
                Zone.DeckTop1 => true,
                Zone.DeckTop2 => true,
                Zone.DeckTop3 => true,
                Zone.DeckTop4 => true,
                Zone.Discard => false,
                Zone.Hand => true,
                Zone.InPlay => false,
                Zone.PlayerMat => true,
                Zone.RecentGains => false,
                Zone.Stash => true,
                Zone.SupplyAll => false,
                Zone.SupplyAvailable => false,
                Zone.SupplyEmpty => false,
                Zone.Trash => false,
                Zone other => throw new NotSupportedException($"Unknown privacy zone {zone}")
            };
        }
    }
}