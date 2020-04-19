using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class Witch : ActionCardBase
    {
        public override CardType[] Types => new[] { CardType.Action, CardType.Attack };
        public override string Art => "dom-witch";
        public override int Cost => 5;

        public override string Text => @"<paras>
            <bold>+2 Cards</bold>
            <run>Each other player gains a Curse.</run>
        </paras>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.DrawCards(2);
            await host.Attack(player => player.Gain("Curse"));
        }
    }
}