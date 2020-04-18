using System;

namespace Cardgame.API
{
    public class Reaction
    {
        public static Reaction None()
        {
            return new Reaction(() => {;}, () => {;});
        }

        public static Reaction Before(Action act)
        {
            return new Reaction(act, () => {;});
        }

        public static Reaction After(Action act)
        {
            return new Reaction(() => {;}, act);
        }

        public static Reaction BeforeAndAfter(Action actBefore, Action actAfter)
        {
            return new Reaction(actBefore, actAfter);
        }

        public readonly Action ActBefore;
        public readonly Action ActAfter;

        private Reaction(Action before, Action after)
        {
            this.ActBefore = before;
            this.ActAfter = after;
        }
    }
}