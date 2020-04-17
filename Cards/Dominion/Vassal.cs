using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class Vassal : ActionCardBase
    {
        public override string Art => "dom-vassal";
        public override int Cost => 3;
        
        public override string Text => @"<paras>
            <block>
                <sym prefix='+'>coin2</sym>
            </block>
            <run>Discard the top card of your deck. If it's an Action card, you may play it.</run>
        </paras>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.AddMoney(2);

            var discarded = host.Examine(Zone.DeckTop1).SingleOrDefault();
            if (discarded != null && discarded.Types.Contains(CardType.Action))
            {
                host.Discard(discarded, Zone.DeckTop1);
                if (await host.YesNo("Vassal", $"<card prefix='Play ' suffix='?'>{discarded.Name}</card>"))
                {
                    await host.PlayCard(discarded.Name, Zone.Discard);
                }
            }
        }
    }
}