using System.Threading.Tasks;

namespace Cardgame.Cards
{
    public class Cellar : ActionCardModel
    {
        public override string Art => "dom-cellar";
        public override int Cost => 2;
        
        public override string Text => @"
        <paras>
            <block>
                <run>+1 Action</run>
            </block>
            <lines>
                <run>Discard any number of cards.</run>
                <run>+1 Card per card discarded.</run>
            </lines>
        </paras>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.AddActions(1);
            var discarded = await host.SelectCardsFromHand("Choose any number of cards to discard.");
            host.DiscardCards(discarded);
            host.DrawCards(discarded.Length);
        }
    }
}