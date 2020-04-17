using System;

namespace Cardgame.API
{
    public class Reaction
    {
        public static Reaction None()
        {
            return new Reaction(ReactionType.None, null);
        }

        public static Reaction Before(Action act)
        {
            return new Reaction(ReactionType.Before, act);
        }

        public static Reaction After(Action act)
        {
            return new Reaction(ReactionType.After, act);
        }

        public static Reaction Replace(Action act)
        {
            return new Reaction(ReactionType.Replace, act);
        }

        public static Reaction Cancel()
        {
            return new Reaction(ReactionType.Replace, () => {;});
        }

        public readonly ReactionType Type;
        public readonly Action Enact;

        private Reaction(ReactionType type, Action act)
        {
            this.Type = type;
            this.Enact = act;
        }
    }
}