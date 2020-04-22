using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Intrigue
{
    public class SecretChamber : ActionReactionCardBase
    {
        public override Trigger ReactionTrigger => Trigger.PlayCard;
        public override int Cost => 2;        

        public override string Text => @"<small>
            <split compact='true'>
                <lines>
                    <run>Discard any number of cards.</run>
                    <spans><sym prefix='+'>coin1</sym><run>per card discarded.</run></spans>
                </lines>
                <run>When another player plays an Attack card, you may reveal this from your hand. If you do, +2 Cards, then put 2 cards from your hand on top of your deck.</run>
            </split>
        </small>";

        protected override async Task ActAsync(IActionHost host)
        {
            var discarded = await host.SelectCards("Choose cards to discard.");
            host.Discard(discarded);
            host.AddCoins(discarded.Length);
        }

        protected override async Task<Reaction> ReactAsync(IActionHost host, string trigger)
        {
            if (!host.IsActive && 
                All.Cards.ByName(trigger).Types.Contains(CardType.Attack) && 
                host.Count(Zone.Hand) >= 5 &&
                await host.YesNo("Secret Chamber", $@"<run>Reveal</run><card>SecretChamber</card><run>from your hand?</run>"))
            {
                host.Reveal("SecretChamber");
                host.IndentLevel++;
                return Reaction.Before(async () => 
                {
                    host.DrawCards(2);
                    var put = await host.SelectCards("Choose cards to put back.", Zone.Hand, 2, 2);
                    host.PlaceOnDeck(put);
                    var ordered = await host.OrderCards("Choose the order of the cards put back.", Zone.DeckTop2);
                    host.Reorder(ordered, Zone.DeckTop2);
                });
            }
            else
            {
                return Reaction.None();
            }
        }
    }
}