using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Seaside
{
    public class Haven : DurationCardBase
    {
        public override int Cost => 2;

        public override string Text => @"<paras>
            <lines>
                <bold>+1 Card</bold>
                <bold>+1 Action</bold>
            </lines>
            <run>Set aside a card from your hand face down (under this). At the start of your next turn, put it into your hand.</run>
        </paras>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.DrawCards(1);
            host.AddActions(1);

            var setAside = await host.SelectCard("Choose a card to set aside.");
            if (setAside != null)
            {
                host.Attach(setAside, Zone.Hand);
            }
            else
            {
                host.CompleteDuration();
            }
        }

        protected override void OnBeginTurn(IActionHost host)
        {
            host.Detach(Zone.Hand);
        }
    }
}