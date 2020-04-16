using System;
using System.Threading.Tasks;

namespace Cardgame.API
{
    public interface IActionCard : ICard
    {
        string SubType { get; }

        Task ExecuteActionAsync(IActionHost host);
    }
}