using System.Threading.Tasks;

namespace Cardgame.API
{
    public interface IReactor
    {
        Task<Reaction> ExecuteReactionAsync(IActionHost host, Trigger trigger, string parameter);
    }
}