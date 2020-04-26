using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Intrigue
{
    public class Saboteur : AttackCardBase
    {
        public override Cost Cost => 5;

        public override string Text => @"<small>
            <run>Each other player reveals cards from the top of their deck until revealing one costing</run>
            <sym>coin3</sym>
            <run>or more. They trash that card and may gain a card costing at most</run>
            <sym>coin2</sym>
            <run>less than it. They discard the other revealed cards.</run>
        </small>";

        protected override async Task ActAsync(IActionHost host)
        {
            await host.Attack(async target =>
            {
                var revealed = target.RevealUntil(card => card.GetCost(host).GreaterThan(2));
                if (revealed.Any())
                {
                    var trashed = revealed.Last();
                    target.Trash(trashed, Zone.Revealed);

                    var gained = await target.SelectCards(
                        "Choose a card to gain, or none.", 
                        Zone.SupplyAvailable, 
                        card => card.GetCost(host).LessThanOrEqual(trashed.GetCost(host).Minus(2)), 
                        0, 1
                    );
                    if (gained.Any())
                    {
                        target.Gain(gained.Single());
                    }

                    target.Discard(revealed.Take(revealed.Length - 1).ToArray(), Zone.Revealed);
                }
            });
        }
    }
}