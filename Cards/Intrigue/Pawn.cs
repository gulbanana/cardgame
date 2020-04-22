using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Intrigue
{
    public class Pawn : ActionCardBase
    {
        public override string Art => "int-pawn";
        public override int Cost => 2;

        public override string Text => @"<lines>
            <spans>        
                <run>Choose two:</run>
                <bold><run>+1 Card;</run></bold>
                <bold><run>+1 Action;</run></bold>
                <bold><run>+1 Buy;</run></bold>
                <sym prefix='+' suffix='.'>coin1</sym>
            </spans>
            <run>The choices must be different.</run>
        </lines>";

        protected override async Task ActAsync(IActionHost host)
        {
            await host.ChooseMultiple("Pawn", 2,
                new NamedOption("<bold>+1 Card</bold>", () => host.DrawCards(1)),
                new NamedOption("<bold>+1 Action</bold>", () => host.AddActions(1)),
                new NamedOption("<bold>+1 Buy</bold>", () => host.AddBuys(1)),
                new NamedOption("<bold><sym prefix='+'>coin1</sym></bold>", () => host.AddCoins(1))
            );
        }
    }
}