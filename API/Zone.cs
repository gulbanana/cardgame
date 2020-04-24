using System;

namespace Cardgame.API
{
    public struct Zone : IEquatable<Zone>
    {
        public static Zone Create = new Zone(nameof(Create));
        public static Zone CountableDeck = new Zone(nameof(CountableDeck));
        public static Zone DeckBottom = new Zone(nameof(DeckBottom));
        public static Zone DeckTop1 = new Zone(nameof(DeckTop1));
        public static Zone DeckTop2 = new Zone(nameof(DeckTop2));
        public static Zone DeckTop3 = new Zone(nameof(DeckTop3));
        public static Zone DeckTop4 = new Zone(nameof(DeckTop4));
        public static Zone Discard = new Zone(nameof(Discard));
        public static Zone Hand = new Zone(nameof(Hand));
        public static Zone InPlay = new Zone(nameof(InPlay));
        public static Zone PlayerMat(string mat) => new Zone(nameof(PlayerMat), mat);
        public static Zone RecentBuys(string player) => new Zone(nameof(RecentBuys), player);
        public static Zone RecentGains(string player) => new Zone(nameof(RecentGains), player);
        public static Zone Stash = new Zone(nameof(Stash));
        public static Zone SupplyAll = new Zone(nameof(SupplyAll));
        public static Zone SupplyAvailable = new Zone(nameof(SupplyAvailable));
        public static Zone SupplyEmpty = new Zone(nameof(SupplyEmpty));
        public static Zone This = new Zone(nameof(This));
        public static Zone Trash = new Zone(nameof(Trash));

        public string Name { get; }
        public object Param { get; }

        private Zone(string name, object param)
        {
            Name = name;
            Param = param;
        }

        private Zone(string name)
        {
            Name = name;
            Param = null;
        }

        public override string ToString()
        {
            if (Param == null) return Name;
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
                nameof(Zone.CountableDeck) => true,
                nameof(Zone.Create) => true,
                nameof(Zone.DeckBottom) => true,
                nameof(Zone.DeckTop1) => true,
                nameof(Zone.DeckTop2) => true,
                nameof(Zone.DeckTop3) => true,
                nameof(Zone.DeckTop4) => true,
                nameof(Zone.Discard) => false,
                nameof(Zone.Hand) => true,
                nameof(Zone.InPlay) => false,
                nameof(Zone.PlayerMat) => true,
                nameof(Zone.RecentGains) => false,
                nameof(Zone.Stash) => true,
                nameof(Zone.SupplyAll) => false,
                nameof(Zone.SupplyAvailable) => false,
                nameof(Zone.SupplyEmpty) => false,
                nameof(Zone.Trash) => false,
                string unknown => throw new NotSupportedException($"Unknown privacy zone {unknown}")
            };
        }
    }
}