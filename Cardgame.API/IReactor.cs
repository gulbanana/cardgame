using System.Threading.Tasks;

namespace Cardgame.API
{
    public interface IReactor
    {
        Task ExecuteReactionAsync(IActionHost host, Zone reactFrom, Trigger triggerType, string triggerParameter);
    }
}