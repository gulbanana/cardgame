using System;

namespace Cardgame.API
{
    public struct Zone : IEquatable<Zone>
    {
        public static Zone Create = new Zone(ZoneName.Create);
        public static Zone Deck = new Zone(ZoneName.Deck);
        public static Zone DeckBottom = new Zone(ZoneName.DeckBottom);
        public static Zone DeckTop1 = new Zone(ZoneName.DeckTop, 1);
        public static Zone DeckTop2 = new Zone(ZoneName.DeckTop, 2);
        public static Zone DeckTop3 = new Zone(ZoneName.DeckTop, 3);
        public static Zone DeckTop4 = new Zone(ZoneName.DeckTop, 4);
        public static Zone DeckTop5 = new Zone(ZoneName.DeckTop, 5);
        public static Zone Discard = new Zone(ZoneName.Discard);
        public static Zone Hand = new Zone(ZoneName.Hand);
        public static Zone InPlay = new Zone(ZoneName.InPlay);
        public static Zone PlayerMat(string mat) => new Zone(ZoneName.PlayerMat, mat);
        public static Zone RecentBuys(string player) => new Zone(ZoneName.RecentBuys, player);
        public static Zone RecentGains(string player) => new Zone(ZoneName.RecentGains, player);
        public static Zone Stash = new Zone(ZoneName.Stash);
        public static Zone SupplyAll = new Zone(ZoneName.Supply, (includeAvailable: true, includeEmpty: true));
        public static Zone SupplyAvailable = new Zone(ZoneName.Supply, (includeAvailable: true, includeEmpty: false));
        public static Zone SupplyEmpty = new Zone(ZoneName.Supply, (includeAvailable: false, includeEmpty: true));
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
                ZoneName.Create => true,
                ZoneName.Deck => true,
                ZoneName.DeckBottom => true,
                ZoneName.DeckTop => true,
                ZoneName.Discard => false,
                ZoneName.Hand => true,
                ZoneName.InPlay => false,
                ZoneName.PlayerMat => true,
                ZoneName.RecentBuys => false,
                ZoneName.RecentGains => false,
                ZoneName.Stash => true,
                ZoneName.Supply => false,
                ZoneName.Trash => false,
                ZoneName unknown => throw new NotSupportedException($"Unknown privacy zone {unknown}")
            };
        }
    }
}