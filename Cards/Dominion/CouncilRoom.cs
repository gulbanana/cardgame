using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class CouncilRoom : ActionCardModel
    {
        public override string Art => "dom-council-room";
        public override int Cost => 5;
        
        public override string Text => @"<paras>
            <block>
                <lines>
                    <run>+4 Cards</run>
                    <run>+1 Buy</run>
                </lines>
            </block>
            <run>Each other player draws a card.</run>
        </paras>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.DrawCards(4);
            host.AddBuys(1);

            await host.Attack(target => target.DrawCards(1), benign: true);
        }
    }
}