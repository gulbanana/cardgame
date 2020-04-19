using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Intrigue
{
    public class Diplomat : ActionReactionCardBase
    {
        public override Trigger ReactionTrigger => Trigger.PlayCard;
        public override string Art => "int-diplomat";
        public override int Cost => 4;        

        public override string Text => @"<split compact='true'>
            <lines>
                <bold>+2 Cards</bold>
                <small>
                    <run>If you have 5 or fewer cards in hand (after drawing),</run>
                    <bold>+2 Actions.</bold>
                </small>
            </lines>
            <small>When another player plays an Attack card, you may first reveal this from a hand of 5 or more cards, to draw 2 cards then discard 3.</small>
        </split>";

        protected override void Act(IActionHost host)
        {
            host.DrawCards(2);
            if (host.Count(Zone.Hand) <= 5)
            {
                host.AddActions(2);
            }
        }

        protected override async Task<Reaction> ReactAsync(IActionHost host, string trigger)
        {
            if (!host.IsActive && 
                All.Cards.ByName(trigger).Types.Contains(CardType.Attack) && 
                host.Count(Zone.Hand) >= 5 &&
                await host.YesNo("Diplomat", $@"<run>Reveal</run><card>Diplomat</card><run>from your hand?</run>"))
            {
                host.Reveal("Diplomat");
                host.IndentLevel++;
                return Reaction.Before(async () => 
                {
                    host.DrawCards(2);
                    var discarded = await host.SelectCards("Choose cards to discard.", Zone.Hand, 3, 3);
                    host.Discard(discarded);
                });
            }
            else
            {
                return Reaction.None();
            }
        }
    }
}