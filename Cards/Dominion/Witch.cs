using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class Witch : ActionCardBase
    {
        public override string SubType => "Attack";
        public override string Art => "dom-witch";
        public override int Cost => 5;

        public override string Text => @"<paras>
            <block>
                <run>+2 Cards</run>
            </block>
            <run>Each other player gains a Curse.</run>
        </paras>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.DrawCards(2);
            await host.Attack(player => player.Gain("Curse"));
        }
    }
}