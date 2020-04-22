using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class Spy : ActionAttackCardBase
    {
        public override int Cost => 4;

        public override string Text => @"<paras>
            <lines>
                <bold>+1 Card</bold>
                <bold>+1 Action</bold>
            </lines>
            <small>Each player (including you) reveals the top card of their deck and either discards it or puts it back, your choice.</small>
        </paras>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.DrawCards(1);
            host.AddActions(1);

            await host.AllPlayers(async target =>
            {
                var revealed = target.Reveal(Zone.DeckTop1).SingleOrDefault();
                if (revealed != null)
                {
                    target.Reveal(revealed, Zone.DeckTop1);
                    
                    var subject = host == target ? "<run>Do you want</run>" : $"<run>Force</run><player>{target.Player}</player>";
                    if (await host.YesNo("Spy", $@"{subject}
                        <run>to discard</run>
                        <card suffix='?'>{revealed.Name}</card>"))
                    {
                        target.Discard(revealed, Zone.DeckTop1);
                    }
                }
            }, isAttack: true);
        }
    }
}