using System;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class Poacher : ActionCardBase
    {
        public override string Art => "dom-poacher";
        public override int Cost => 4;

        public override string Text => @"<paras>
            <bold>
                <lines>
                    <run>+1 Card</run>
                    <run>+1 Action</run>
                    <sym prefix='+'>coin1</sym>
                </lines>
            </bold>
            <run>Discard a card per empty Supply pile.</run>
        </paras>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.DrawCards(1);
            host.AddActions(1);
            host.AddMoney(1);

            var emptyPiles = host.Count(Zone.SupplyEmpty);
            var handSize = host.Count(Zone.Hand);
            var toDiscard = Math.Min(emptyPiles, handSize);

            if (toDiscard > 0)
            {
                var discarded = await host.SelectCards(
                    toDiscard == 1 ? "Choose a card to discard" : $"Choose {toDiscard} cards to discard.", 
                    toDiscard, 
                    toDiscard
                );
                host.Discard(discarded);
            }
        }
    }
}