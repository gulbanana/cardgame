using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Intrigue
{
    public class SecretPassage : ActionCardBase
    {
        public override string Art => "int-secret-passage";
        public override int Cost => 4;

        public override string Text => @"<paras>
            <lines>
                <bold>+2 Cards</bold>
                <bold>+1 Action</bold>
            </lines>
            <run>Take a card from your hand and put it anywhere in your deck.</run>
        </paras>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.DrawCards(2);
            host.AddActions(1);

            var inserted = await host.SelectCard("Choose a card to put in your deck.", Zone.Hand);

            if (host.Count(Zone.DeckTop1) == 0)
            {
                host.PutOnDeck(inserted);
            }
            else
            {
                var deckSize = host.Count(Zone.CountableDeck);
                var options = Enumerable.Range(0, deckSize+1).Select(i =>
                {
                    var label = 
                        i == 0 ? "Top" :
                        i == deckSize ? "Bottom" :
                        i.ToString();
                    return new NamedOption(label, () => host.InsertIntoDeck(inserted.Name, i));
                });
                await host.ChooseOne($"Choose where to put {inserted.Name}.", options.ToArray());
            }
        }
    }
}