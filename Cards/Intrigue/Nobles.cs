using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Intrigue
{
    public class Nobles : ActionCardBase, IVictoryCard
    {
        public override CardType[] Types => new[] { CardType.Action, CardType.Victory };
        public override string Art => "int-nobles";
        public override int Cost => 6;

        public override string Text => @"<split>
            <spans>
                <run>Choose one:</run>
                <block><run>+3 Cards;</run></block>
                <run>or</run>
                <block><run>+2 Actions.</run></block>
            </spans>
            <block>
                <sym large='true' prefix='2'>vp</sym>
            </block>
        </split>";

        protected override async Task ActAsync(IActionHost host)
        {
            await host.ChooseOne("Nobles", 
                ("+3 Cards", () => host.DrawCards(3)),
                ("+2 Actions", () => host.AddActions(2))
            );
        }

        public int Score(string[] dominion) => 2;
    }
}