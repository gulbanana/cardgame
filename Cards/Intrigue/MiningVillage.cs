using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Intrigue
{
    public class MiningVillage : ActionCardBase
    {
        public override string Art => "int-mining-village";
        public override int Cost => 4;
        
        public override string Text => @"<lines>
            <bold>+1 Card</bold>
            <bold>+2 Actions</bold>
        </lines>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.DrawCards(1);
            host.AddActions(2);

            if (await host.YesNo("Mining Village", "<run>Trash</run><card>MiningVillage</card><run>for</run><sym prefix='+' suffix='?'>coin2</sym>"))
            {
                host.Trash("MiningVillage", Zone.InPlay);
                host.AddCoins(2);
            }
        }
    }
}