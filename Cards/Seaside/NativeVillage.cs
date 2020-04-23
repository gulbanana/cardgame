using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Seaside
{
    public class NativeVillage : ActionCardBase
    {
        public override int Cost => 2;
        public override string HasMat => "NativeVillageMat";

        public override string Text => @"<lines>
            <bold>+2 Actions</bold>
            <small>Choose one: Put the top card of your deck face down on your Native Village mat (you may look at those cards at any time); or put all the cards from your mat into your hand.</small>
        </lines>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.AddActions(2);

            var cards = host.Examine(Zone.PlayerMat, "NativeVillageMat");

            await host.ChooseOne("Native Village",
                new NamedOption("Add card to mat.", () =>
                {                    
                    var topDeck = host.Examine(Zone.DeckTop1).Single();
                    host.PutOnMat("NativeVillageMat", topDeck, Zone.DeckTop1);
                }),
                new NamedOption($"Retrieve all cards ({(cards.Any() ? cards.Length.ToString() : "none")}).", () =>
                {
                    host.PutIntoHand(Zone.PlayerMat, "NativeVillageMat");
                })
            );
        }
    }
}