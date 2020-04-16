using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class Spy : ActionCardModel
    {
        public override string SubType => "Attack";
        public override string Art => "dom-spy";
        public override int Cost => 4;

        public override string Text => @"<paras>
            <block>
                <lines>
                    <run>+1 Card</run>
                    <run>+1 Action</run>
                </lines>
            </block>
            <run>Each player (including you) reveals the top 2 card of their deck and either discards it or puts it back, your choice.</run>
        </paras>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.DrawCards(1);
            host.AddActions(1);

            await SpyOn(host, host);            
            await host.Attack(player => SpyOn(host, player));
        }

        private async Task SpyOn(IActionHost host, IActionHost target)
        {
            var revealed = target.RevealAll(Zone.DeckTop1).SingleOrDefault();
            if (revealed != null)
            {
                var subject = host == target ? "<run>Do you want</run>" : $"<run>Force</run><player>{target.Player}</player>";
                if (await host.YesNo("Spy", $@"<spans>
                    {subject}
                    <run>to discard</run>
                    <card suffix='?'>{revealed.Name}</card>
                </spans>"))
                {
                    target.Discard(revealed, Zone.DeckTop1);
                }
            }
        }
    }
}