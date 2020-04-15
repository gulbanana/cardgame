using System.Threading.Tasks;

namespace Cardgame.Cards.Dominion
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
                <run>Discard any number of cards, then draw that many.</run>
            </lines>
        </paras>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.AddActions(1);
            var discarded = await host.SelectCards("Choose any number of cards to discard.");
            host.Discard(discarded);
            host.DrawCards(discarded.Length);
        }
    }
}