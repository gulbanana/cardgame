using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Alchemy
{
    public class ScryingPool : AttackCardBase
    {
        public override Cost Cost => new Cost(2, true);

        public override string Text => @"<lines>
            <bold>+1 Action</bold>
            <small>Each player (including you) reveals the top card of their deck and either discards it or puts it back, your choice. Then reveal cards from your deck until revealing one that isn’t an Action. Put all of those revealed cards into your hand.</small>
        </lines>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.AddActions(1);

            await host.AllPlayers(async target =>
            {
                var revealed = target.Reveal(Zone.DeckTop(1)).SingleOrDefault();
                if (revealed != null)
                {                    
                    var subject = host == target ? "<run>Do you want</run>" : $"<run>Force</run><player>{target.Player}</player>";
                    if (await host.YesNo("Spy", $@"{subject}
                        <run>to discard</run>
                        <card suffix='?'>{revealed.Name}</card>"))
                    {
                        target.Discard(revealed, Zone.Deck);
                    }
                }
            }, isAttack: true);

            var revealed = host.RevealUntil(card => !card.Types.Contains(CardType.Action));
            host.PutIntoHand(revealed, Zone.Revealed);
        }
    }
}