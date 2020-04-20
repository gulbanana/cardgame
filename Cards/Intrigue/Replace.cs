using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Intrigue
{
    public class Replace : ActionAttackCardBase
    {
        public override string Art => "int-replace";
        public override int Cost => 5;

        public override string Text => @"<small>
            <run>Trash a card from your hand. Gain a card costing up to</run>
            <sym>coin2</sym>
            <run>more than it. If the gained card is an Action or Treasure, put it onto your deck; if it's a Victory card, each other player gains a Curse.</run>
        </small>";

        protected override async Task ActAsync(IActionHost host)
        {
            var trashed = await host.SelectCard("Choose a card to trash.", Zone.Hand);
            if (trashed != null)
            {
                var cost = trashed.GetCost(host);
                host.Trash(trashed);

                var gained = await host.SelectCard("Choose a card to gain.", Zone.SupplyAvailable, card => card.GetCost(host) <= cost + 2);
                if (gained != null)
                {
                    if (gained.Types.Contains(CardType.Action) || gained.Types.Contains(CardType.Treasure))
                    {
                        host.Gain(gained, Zone.DeckTop1);
                    }
                    else
                    {
                        host.Gain(gained);
                    }

                    if (gained.Types.Contains(CardType.Victory))
                    {
                        await host.Attack(target => target.Gain("Curse"));
                    }
                }
            }
        }
    }
}