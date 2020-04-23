using System;
using System.Threading.Tasks;

namespace Cardgame.API
{
    public class Reaction
    {
        public static Reaction None()
        {
            return new Reaction(() => Task.CompletedTask, () => Task.CompletedTask);
        }

        public static Reaction Before(Func<Task> act)
        {
            return new Reaction(act, () => Task.CompletedTask);
        }

        public static Reaction Before(Action act)
        {
            return new Reaction(() =>
            {
                act();
                return Task.CompletedTask;
            }, () => Task.CompletedTask);
        }

        public static Reaction After(Func<Task> act)
        {
            return new Reaction(() => Task.CompletedTask, act);
        }

        public static Reaction After(Action act)
        {
            return new Reaction(() => Task.CompletedTask, () =>
            {
                act();
                return Task.CompletedTask;
            });
        }

        public static Reaction BeforeAndAfter(Action actBefore, Action actAfter)
        {
            return new Reaction(() =>
            {
                actBefore();
                return Task.CompletedTask;
            }, () =>
            {
                actAfter();
                return Task.CompletedTask;
            });
        }

        public static Reaction BeforeAndAfter(Func<Task> actBefore, Func<Task> actAfter)
        {
            return new Reaction(actBefore, actAfter);
        }

        public readonly Func<Task> ActBefore;
        public readonly Func<Task> ActAfter;

        private Reaction(Func<Task> before, Func<Task> after)
        {
            this.ActBefore = before;
            this.ActAfter = after;
        }
    }
}