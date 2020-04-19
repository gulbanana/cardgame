using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class CouncilRoom : ActionCardBase
    {
        public override string Art => "dom-council-room";
        public override int Cost => 5;
        
        public override string Text => @"<paras>
            <lines>
                <bold>+4 Cards</bold>
                <bold>+1 Buy</bold>
            </lines>
            <run>Each other player draws a card.</run>
        </paras>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.DrawCards(4);
            host.AddBuys(1);

            await host.OtherPlayers(target => target.DrawCards(1));
        }
    }
}