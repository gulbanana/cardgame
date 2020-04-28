using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Intrigue
{
    public class Nobles : VictoryActionCardBase
    {
        public override string Art => "int-nobles";
        public override Cost Cost => 6;
        public override int Score => 2;

        public override string Text => @"<split>
            <spans>
                <run>Choose one:</run>
                <bold>+3 Cards;</bold>
                <run>or</run>
                <bold>+2 Actions.</bold>
            </spans>
            <bold><sym large='true'>2vp</sym></bold>
        </split>";

        protected override async Task ActAsync(IActionHost host)
        {
            await host.ChooseOne("Nobles", 
                new NamedOption("+3 Cards", () => host.DrawCards(3)),
                new NamedOption("+2 Actions", () => host.AddActions(2))
            );
        }
    }
}