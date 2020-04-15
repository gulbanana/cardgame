using System.Threading.Tasks;

namespace Cardgame.Cards.Dominion
{
    public class Chancellor : ActionCardModel
    {
        public override string Art => "dom-chancellor";
        public override int Cost => 3;
        
        public override string Text => @"<paras>
            <block>
                <sym prefix='+'>coin2</sym>
            </block>
            <run>You may immediately put your deck into your discard pile.</run>
        </paras>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.AddMoney(2);
            if (await host.YesNo("Chancellor", $@"<run>Do you want to put your deck into your discard pile?</run>"))
            {
                host.DiscardEntireDeck();
            }
        }
    }
}