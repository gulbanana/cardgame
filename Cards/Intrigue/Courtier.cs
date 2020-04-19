using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Intrigue
{
    public class Courtier : ActionCardBase
    {
        public override string Art => "int-courtier";
        public override int Cost => 5;

        public override string Text => @"<spans>
            <run>Reveal a card from your hand. For each type it has (Action, Attack, etc.), choose one;</run>
            <bold>+1 Action;</bold>
            <run>or</run>
            <bold>+1 Buy;</bold>
            <run>or</run>
            <sym prefix='+' suffix=';'>coin3</sym>
            <run>or gain a Gold. The choices must be different.</run>
        </spans>";

        protected override async Task ActAsync(IActionHost host)
        {
            var revealed = await host.SelectCard("Choose a card to reveal.");
            if (revealed != null)
            {
                host.Reveal(revealed);
                await host.ChooseMultiple("Courtier", revealed.Types.Length,
                    new NamedOption("<bold>+1 Action</bold>", () => host.AddActions(1)),
                    new NamedOption("<bold>+1 Buy</bold>", () => host.AddBuys(1)),                    
                    new NamedOption("<bold><sym prefix='+'>coin3</sym></bold>", () => host.AddMoney(3)),
                    new NamedOption("<run>Gain a</run><card>Gold</card>", () => host.Gain("Gold"))
                );
            }
        }
    }
}