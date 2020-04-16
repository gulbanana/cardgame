using System.Threading.Tasks;

namespace Cardgame.API
{
    public interface IReactor
    {
        string Name { get; }
        Task<Reaction> ExecuteReactionAsync(IActionHost host, TriggerType triggerType, string triggerParameter);
    }
}