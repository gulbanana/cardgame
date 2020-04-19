using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class Cellar : ActionCardBase
    {
        public override string Art => "dom-cellar";
        public override int Cost => 2;
        
        public override string Text => @"<paras>
            <bold>+1 Action</bold>
            <run>Discard any number of cards, then draw that many.</run>
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