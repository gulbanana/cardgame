using System;
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
                <bold>+3 Cards;</bold>
                <run>or</run>
                <bold>+2 Actions.</bold>
            </spans>
            <bold><sym large='true' prefix='2'>vp</sym></bold>
        </split>";

        protected override async Task ActAsync(IActionHost host)
        {
            await host.ChooseOne("Nobles", 
                new NamedOption("+3 Cards", () => host.DrawCards(3)),
                new NamedOption("+2 Actions", () => host.AddActions(2))
            );
        }

        public int Score(string[] dominion) => 2;
    }
}