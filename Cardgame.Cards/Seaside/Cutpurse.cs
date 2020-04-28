using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Seaside
{
    public class Cutpurse : AttackCardBase
    {
        public override Cost Cost => 4;

        public override string Text => @"<paras>
            <bold><sym>+coin2</sym></bold>
            <run>Each other player discards a Copper (or reveals a hand with no Copper).</run>
        </paras>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.AddCoins(2);

            await host.Attack(target =>
            {
                var hand = target.Examine(Zone.Hand);
                if (hand.Any(card => card.Name == "Copper"))
                {
                    target.Discard("Copper");
                }
                else
                {
                    target.Reveal(Zone.Hand);
                }
            });
        }
    }
}