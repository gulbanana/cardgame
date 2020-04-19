using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Intrigue
{
    public class Mill : ActionVictoryCardBase
    {
        public override string Art => "int-mill";
        public override int Cost => 4;
        protected override int Score => 1;

        public override string Text => @"<split compact='true'>
            <lines>
                <bold>+1 Card</bold>
                <bold>+1 Action</bold>
                <spans>
                    <run>You may discard 2 cards, for</run>
                    <sym prefix='+' suffix='.'>coin2</sym>
                </spans>
            </lines>
            <bold><sym large='true' prefix='1'>vp</sym></bold>
        </split>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.DrawCards(1);
            host.AddActions(1);

            var handSize = host.Count(Zone.Hand);
            if (handSize >= 2 && await host.YesNo("Mill", "<run>Discard 2 cards for</run><sym prefix='+' suffix='?'>coin2</sym>"))
            {
                if (handSize > 2)
                {
                    var discarded = await host.SelectCards("Choose cards to discard", 2, 2);
                    host.Discard(discarded);
                }
                else
                {
                    host.Discard(Zone.Hand);
                }

                if (handSize > 1)
                {
                    host.AddMoney(2);
                }
            }            
        }
    }
}