using System.Threading.Tasks;

namespace Cardgame.API
{
    public interface IActionCard : ICard
    {
        Task ExecuteActionAsync(IActionHost host);
    }
}