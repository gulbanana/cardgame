using System;
using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards
{
    public abstract class AttackReactionCardBase : ReactionCardBase
    {
        public sealed override Trigger ReactionTrigger => Trigger.PlayCard;

        protected sealed override async Task<Reaction> ReactAsync(IActionHost host, string trigger)
        {
            if (!host.IsActive && 
                All.Cards.ByName(trigger).Types.Contains(CardType.Attack) && 
                await host.YesNo(Name, $@"<run>Reveal</run><card>{Name}</card><run>from your hand?</run>"))
            {
                host.Reveal(Name);
                host.IndentLevel++;
                
                return Reaction.BeforeAndAfter(
                    () => BeforeAttackAsync(host),
                    () => AfterAttackAsync(host)
                );
            }
            else
            {
                return Reaction.None();
            }
        }

        protected virtual void BeforeAttack(IActionHost host) { }

        protected virtual Task BeforeAttackAsync(IActionHost host) 
        { 
            BeforeAttack(host); 
            return Task.CompletedTask; 
        }
        
        protected virtual void AfterAttack(IActionHost host) { }

        protected virtual Task AfterAttackAsync(IActionHost host) 
        { 
            AfterAttack(host); 
            return Task.CompletedTask; 
        }
    }
}