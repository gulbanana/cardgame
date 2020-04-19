using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class Harbinger : ActionCardBase
    {
        public override string Art => "dom-harbinger";
        public override int Cost => 3;
        
        public override string Text => @"<paras>
            <lines>
                <bold>+1 Card</bold>
                <bold>+1 Action</bold>
            </lines>
            <run>Look through your discard pile. You may put a card from it onto your deck.</run>
        </paras>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.DrawCards(1);
            host.AddActions(1);

            var selected = await host.SelectCards("Choose up to one card from your discard pile.", Zone.Discard, 0, 1);
            if (selected.Any())
            {
                host.PlaceOnDeck(selected.Single(), Zone.Discard);
            }
        }
    }
}