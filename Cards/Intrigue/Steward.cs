using System;
using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Intrigue
{
    public class Steward : ActionCardBase
    {
        public override string Art => "int-steward";
        public override int Cost => 3;

        public override string Text => @"<spans>
            <run>Choose one:</run>
            <bold>+2 Cards;</bold>
            <run>or</run>
            <bold><sym prefix='+' suffix=';'>coin2</sym></bold>
            <run>or trash 2 cards from your hand.</run>
        </spans>";

        protected override async Task ActAsync(IActionHost host)
        {
            await host.ChooseOne("Steward", 
                new NamedOption("+2 Cards", () => host.DrawCards(2)),
                new NamedOption("<sym prefix='+'>coin2</sym>", () => host.AddMoney(2)),
                new NamedOption("Trash 2 cards", async () => 
                {
                    var handSize = host.Examine(Zone.Hand).Count();
                    if (handSize > 2)
                    {
                        var trashed = await host.SelectCards("Choose cards to trash.", Zone.Hand, Math.Min(handSize, 2), Math.Min(handSize, 2));
                        host.Trash(trashed);
                    }
                    else
                    {
                        host.Trash(Zone.Hand);
                    }
                })
            );
        }

        public int Score(string[] dominion) => 2;
    }
}