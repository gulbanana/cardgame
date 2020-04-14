using System.Threading.Tasks;

namespace Cardgame.Cards
{
    public class Militia : ActionCardModel
    {
        public override string SubType => "Attack";
        public override string Art => "dom-militia";
        public override int Cost => 4;        

        public override TextModel Text => TextModel.Parse(@"
        <paras>
            <block>
                <sym prefix='+'>coin2</sym>
            </block>
            <run>Each other player discards down to 3 cards in his hand.</run>
        </paras>");

        protected override async Task ActAsync(IActionHost host)
        {
            host.AddMoney(2);

            await host.Attack(player => player.GetHandCards() > 3, async player =>
            {
                var n = player.GetHandCards() - 3;
                var discardedCards = await player.SelectCardsFromHand(n == 1 ? "Choose a card to discard" : $"Choose {n} cards to discard.", n);
                player.DiscardCards(discardedCards);
            });
        }
    }
}