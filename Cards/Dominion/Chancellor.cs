using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class Chancellor : ActionCardBase
    {
        public override int Cost => 3;
        
        public override string Text => @"<paras>
            <bold><sym prefix='+'>coin2</sym></bold>
            <run>You may immediately put your deck into your discard pile.</run>
        </paras>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.AddCoins(2);
            if (await host.YesNo("Chancellor", "Do you want to put your deck into your discard pile?"))
            {
                host.DiscardEntireDeck();
            }
        }
    }
}