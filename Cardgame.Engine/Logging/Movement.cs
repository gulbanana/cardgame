using Cardgame.API;

namespace Cardgame.Engine.Logging
{
    public class Movement
    {
        public readonly Motion Type;
        public readonly string[] Cards;
        public readonly Zone From;
        public readonly Zone To;

        public Movement(Motion type, string[] cards, Zone from, Zone to)
        {
            Type = type;
            Cards = cards;
            From = from;
            To = to;
        }
    }
}