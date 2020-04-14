using System.Linq;
using System.Threading.Tasks;

namespace Cardgame.Cards
{
    public class Workshop : ActionCardModel
    {
        public override string Art => "dom-workshop";
        public override int Cost => 3;
        
        public override TextModel Text => TextModel.Parse(@"
        <spans>
            <run>Gain a card costing up to</run>
            <sym>coin4</sym>
            <run>.</run>
        </spans>");

        protected override async Task ActAsync(IActionHost host)
        {
            var gainedCard = await host.SelectCard(
                "Choose a card to gain.", 
                CardSource.Kingdom, 
                cards => cards.Where(card => card.Cost <= 4)
            );
            host.GainCard(gainedCard.Name);
        }
    }
}