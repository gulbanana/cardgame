using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class Moneylender : ActionCardBase
    {
        public override string Art => "dom-moneylender";
        public override int Cost => 4;
        
        public override string Text => @"<spans>
            <run>You may trash a Coppper from your hand for</run>
            <sym prefix='+' suffix='.'>coin3</sym>
        </spans>";

        protected override async Task ActAsync(IActionHost host)
        {
            if (host.Examine(Zone.Hand).Any(card => card is Base.Copper))
            {
                if (await host.YesNo("Moneylender", $@"Do you want to trash a Copper?"))
                {
                    host.Trash("Copper");
                    host.AddMoney(3);
                }
            }
        }
    }
}