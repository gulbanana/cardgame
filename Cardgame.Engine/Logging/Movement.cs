using Cardgame.API;

namespace Cardgame.Engine.Logging
{
    public class Movement
    {
        public Motion Type { get; }
        public string[] Cards { get; }
        public Zone From { get; }
        public Zone To { get; }

        public Movement(Motion type, string[] cards, Zone from, Zone to)
        {
            Type = type;
            Cards = cards;
            From = from;
            To = to;
        }
    }
}