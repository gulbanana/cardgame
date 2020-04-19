using System;
using System.Threading.Tasks;

namespace Cardgame.API
{
    public struct NamedOption
    {
        public readonly string Text;
        public readonly Func<Task> Execute;

        public NamedOption(string text, Func<Task> execute)
        {
            Text = text;
            Execute = execute;
        }

        public NamedOption(string text, Action execute)
        {
            Text = text;
            Execute = () =>
            {
                execute();
                return Task.CompletedTask;
            };
        }
    }
}