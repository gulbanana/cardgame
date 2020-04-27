using System;
using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards
{
    public abstract class AttackReactionCardBase : ReactionCardBase
    {
        public sealed override Trigger ReactionTrigger => Trigger.BeforePlayCard;

        protected sealed override async Task ReactAsync(IActionHost host, string trigger)
        {
            if (!host.IsActive && 
                AllCards.ByName(trigger).Types.Contains(CardType.Attack) && 
                await host.YesNo(Name, $@"<run>Reveal</run><card>{Name}</card><run>from your hand?</run>"))
            {
                host.Reveal(Name);
                await ReactAsync(host.Isolate());
            }
        }

        protected virtual Task ReactAsync(IActionHost host) 
        {
            React(host);
            return Task.CompletedTask;
        }

        protected virtual void React(IActionHost host) => throw new NotImplementedException($"Reaction not implemented.");
    }
}