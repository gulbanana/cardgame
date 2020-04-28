using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Intrigue
{
    public class SecretChamber : AttackReactionCardBase
    {
        public override Cost Cost => 2;        

        public override string Text => @"<small>
            <split compact='true'>
                <lines>
                    <run>Discard any number of cards.</run>
                    <spans><sym>+coin1</sym><run>per card discarded.</run></spans>
                </lines>
                <run>When another player plays an Attack card, you may reveal this from your hand. If you do, +2 Cards, then put 2 cards from your hand on top of your deck.</run>
            </split>
        </small>";

        protected override async Task ActAsync(IActionHost host)
        {
            var discarded = await host.SelectCards("Choose cards to discard.");
            host.Discard(discarded);
            host.AddCoins(discarded.Length);
        }

        protected override async Task ReactAsync(IActionHost host)
        {
            host.DrawCards(2);
            var put = await host.SelectCards("Choose cards to put back.", Zone.Hand, 2, 2);
            host.PutOnDeck(put);
            var ordered = await host.OrderCards("Choose the order of the cards put back.", Zone.DeckTop(2));
            host.Reorder(ordered, Zone.Deck);
        }
    }
}