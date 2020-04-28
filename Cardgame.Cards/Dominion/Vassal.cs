using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class Vassal : ActionCardBase
    {
        public override string Art => "dom-vassal";
        public override Cost Cost => 3;
        
        public override string Text => @"<paras>
            <bold>
                <sym>+coin2</sym>
            </bold>
            <run>Discard the top card of your deck. If it's an Action card, you may play it.</run>
        </paras>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.AddCoins(2);

            var discarded = host.Examine(Zone.DeckTop(1)).SingleOrDefault();
            if (discarded != null && discarded.Types.Contains(CardType.Action))
            {
                host.Discard(discarded, Zone.Deck);
                if (await host.YesNo("Vassal", $"<card prefix='Play ' suffix='?'>{discarded.Name}</card>"))
                {
                    await host.PlayCard(discarded.Name, Zone.Discard);
                }
            }
        }
    }
}