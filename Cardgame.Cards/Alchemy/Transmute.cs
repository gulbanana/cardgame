using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Alchemy
{
    public class Transmute : ActionCardBase
    {
        public override Cost Cost => new Cost(0, true);

        public override string Text => @"<paras>
            <lines>
                <run>Trash a card from your hand.</run>
                <run>If it is an...</run>
            </lines>
            <lines>
                <run>Action card, gain a Duchy</run>
                <run>Treasure card, gain a Transmute</run>
                <run>Victory card, gain a Gold</run>
            </lines>
        </paras>";

        protected override async Task ActAsync(IActionHost host)
        {
            var trashed = await host.SelectCard("Choose a card to trash.");
            if (trashed != null)
            {
                if (trashed.Types.Contains(CardType.Action)) await host.Gain("Duchy");
                if (trashed.Types.Contains(CardType.Treasure)) await host.Gain("Transmute");
                if (trashed.Types.Contains(CardType.Victory)) await host.Gain("Gold");
            }
        }
    }
}