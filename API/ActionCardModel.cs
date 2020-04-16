using System;
using System.Threading.Tasks;

namespace Cardgame.API
{
    public interface IActionCard : ICard
    {
        string SubType { get; }
        TriggerType ReactionTrigger { get; }

        Task ExecuteActionAsync(IActionHost host);
        Task<Reaction> ExecuteReactionAsync(IActionHost host, string trigger);
    }
}