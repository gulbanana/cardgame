using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Intrigue
{
    public class Courtyard : ActionCardBase
    {
        public override string Art => "int-courtyard";
        public override Cost Cost => 2;

        public override string Text => @"<paras>
            <bold>+3 Cards</bold>
            <run>Put a card from your hand onto your deck.</run>
        </paras>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.DrawCards(3);

            var put = await host.SelectCard("Choose a card to put onto your deck.");
            host.PutOnDeck(put);
        }
    }
}