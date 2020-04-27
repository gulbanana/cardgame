using System.Threading.Tasks;
using Cardgame.Engine;
using Cardgame.Model;

namespace Cardgame.Tests
{
    internal static class Extensions
    {
        public static Task ExecuteChecked(this GameEngine source, string username, ClientCommand command)
        {
            try
            {
                command.Seq = source.Model.Seq;
                source.ExecuteInternal(username, command);
                return source.Background;
            }
            catch (CommandException ce)
            {
                throw new Xunit.Sdk.XunitException(ce.Message);
            }
        }
    }
}