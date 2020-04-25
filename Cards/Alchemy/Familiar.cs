using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Alchemy
{
    public class Familiar : AttackCardBase
    {
        public override Cost Cost => new Cost(3, true);

        public override string Text => @"<paras>
            <lines>
                <bold>+1 Card</bold>
                <bold>+1 Action</bold>
            </lines>
            <run>Each other player gains a Curse.</run>
        </paras>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.DrawCards(1);
            host.AddActions(1);

            await host.Attack(target => target.Gain("Curse"));
        }
    }
}