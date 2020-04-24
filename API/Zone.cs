using System;

namespace Cardgame.API
{
    public struct Zone : IEquatable<Zone>
    {
        public static Zone Create = new Zone(ZoneName.Create);
        public static Zone CountableDeck = new Zone(ZoneName.CountableDeck);
        public static Zone DeckBottom = new Zone(ZoneName.DeckBottom);
        public static Zone DeckTop1 = new Zone(ZoneName.DeckTop1);
        public static Zone DeckTop2 = new Zone(ZoneName.DeckTop2);
        public static Zone DeckTop3 = new Zone(ZoneName.DeckTop3);
        public static Zone DeckTop4 = new Zone(ZoneName.DeckTop4);
        public static Zone Discard = new Zone(ZoneName.Discard);
        public static Zone Hand = new Zone(ZoneName.Hand);
        public static Zone InPlay = new Zone(ZoneName.InPlay);
        public static Zone PlayerMat(string mat) => new Zone(ZoneName.PlayerMat, mat);
        public static Zone RecentBuys(string player) => new Zone(ZoneName.RecentBuys, player);
        public static Zone RecentGains(string player) => new Zone(ZoneName.RecentGains, player);
        public static Zone Stash = new Zone(ZoneName.Stash);
        public static Zone SupplyAll = new Zone(ZoneName.SupplyAll);
        public static Zone SupplyAvailable = new Zone(ZoneName.SupplyAvailable);
        public static Zone SupplyEmpty = new Zone(ZoneName.SupplyEmpty);
        public static Zone This = new Zone(ZoneName.This);
        public static Zone Trash = new Zone(ZoneName.Trash);

        public ZoneName Name { get; }
        public object Param { get; }

        private Zone(ZoneName name, object param)
        {
            Name = name;
            Param = param;
        }

        private Zone(ZoneName name)
        {
            Name = name;
            Param = null;
        }

        public override string ToString()
        {
            if (Param == null) return Name.ToString();
            return $"{Name}({Param})";
        }

        public bool Equals(Zone other)
        {
            if (this.Name != other.Name) return false;
            if (this.Param is null) return other.Param is null;
            return this.Param.Equals(other.Param);
        }

        public override bool Equals(object obj) => obj is Zone other && this.Equals(other);

        public override int GetHashCode() => Name.GetHashCode();

        public static bool operator ==(Zone a, Zone b) => a.Equals(b);

        public static bool operator !=(Zone a, Zone b) => !a.Equals(b);
    }

    public static class ZoneExtensions
    {
        public static bool IsPrivate(this Zone zone)
        {
            return zone.Name switch 
            {
                ZoneName.CountableDeck => true,
                ZoneName.Create => true,
                ZoneName.DeckBottom => true,
                ZoneName.DeckTop1 => true,
                ZoneName.DeckTop2 => true,
                ZoneName.DeckTop3 => true,
                ZoneName.DeckTop4 => true,
                ZoneName.Discard => false,
                ZoneName.Hand => true,
                ZoneName.InPlay => false,
                ZoneName.PlayerMat => true,
                ZoneName.RecentBuys => false,
                ZoneName.RecentGains => false,
                ZoneName.Stash => true,
                ZoneName.SupplyAll => false,
                ZoneName.SupplyAvailable => false,
                ZoneName.SupplyEmpty => false,
                ZoneName.Trash => false,
                ZoneName unknown => throw new NotSupportedException($"Unknown privacy zone {unknown}")
            };
        }
    }
}