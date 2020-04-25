using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Seaside
{
    public class GhostShip : AttackCardBase
    {
        public override Cost Cost => 5;

        public override string Text => @"<paras>
            <bold>+2 Cards</bold>
            <run>Each other player with 4 or more cards in hand puts cards from their hand onto their deck until they have 3 cards in hand.</run>
        </paras>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.DrawCards(2);
            await host.Attack(target => target.Count(Zone.Hand) >= 4, async target =>
            {
                while (target.Count(Zone.Hand) > 3) {
                    var put = await target.SelectCard("Choose a card to put onto your deck");
                    target.PutOnDeck(put);
                }
            });
        }
    }
}