using System;

namespace Cardgame.API
{
    public struct Cost : IEquatable<Cost>, IComparable<Cost>
    {
        public static implicit operator Cost(int coins) => new Cost { Coins = coins, Potion = false };

        public int Coins { get; set; }
        public bool Potion { get; set; }

        public Cost(int coins, bool potion)
        {
            Coins = coins;
            Potion = potion;
        }

        public override string ToString() => Potion ? Coins+"P" : Coins.ToString();

        public bool Equals(Cost other)
        {
            return this.Potion == other.Potion && this.Coins == other.Coins;
        }

        public int CompareTo(Cost other)
        {
            var result = this.Coins.CompareTo(other.Coins);
            if (result == 0)
            {
                if (this.Potion && !other.Potion)
                {
                    return 1;
                }
                else if (other.Potion && !this.Potion)
                {
                    return -1;
                }
            }
            return result;
        }

        public bool LessThan(Cost other)
        {
            return CompareTo(other) < 0 && (other.Potion || !this.Potion);
        }

        public bool LessThanOrEqual(Cost other)
        {
            return this.Coins <= other.Coins && (other.Potion || !this.Potion);
        }

        public bool GreaterThan(Cost other)
        {
            return CompareTo(other) > 0 && (this.Potion || !other.Potion);
        }

        public bool GreaterThanOrEqual(Cost other)
        {
            return this.Coins >= other.Coins && (this.Potion || !other.Potion);
        }

        public Cost Plus(Cost other)
        {
            if (other.Potion) throw new Exception("Can't add Potions to cost.");
            return new Cost { Coins = this.Coins + other.Coins, Potion = this.Potion };
        }

        public Cost Minus(Cost other)
        {
            if (other.Potion) throw new Exception("Can't remove Potions from cost.");
            return new Cost { Coins = this.Coins - other.Coins, Potion = this.Potion };
        }
    }
}