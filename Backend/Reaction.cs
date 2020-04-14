namespace Cardgame
{
    public class Reaction
    {
        public delegate void Enactor(IActionHost self, IActionHost trigger);

        public static Reaction None()
        {
            return new Reaction(ReactionType.None, null);
        }

        public static Reaction Before(Enactor act)
        {
            return new Reaction(ReactionType.Before, act);
        }

        public static Reaction After(Enactor act)
        {
            return new Reaction(ReactionType.After, act);
        }

        public static Reaction Replace(Enactor act)
        {
            return new Reaction(ReactionType.Replace, act);
        }

        public static Reaction Cancel()
        {
            return new Reaction(ReactionType.Replace, (self, attacker) => { ;});
        }

        public readonly ReactionType Type;
        public readonly Enactor Enact;

        private Reaction(ReactionType type, Enactor act)
        {
            this.Type = type;
            this.Enact = act;
        }
    }
}