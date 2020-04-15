using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cardgame.Cards.Dominion
{
    public class Library : ActionCardModel
    {
        public override string Art => "dom-library";
        public override int Cost => 5;
        
        public override string Text => @"<run>
            Draw until you have 7 cards in hand, skipping any Action cards you choose to; set those aside, discarding them afterwards.
        </run>";

        protected override async Task ActAsync(IActionHost host)
        {
            var setAside = new List<string>();

            while (host.GetHand().Count() < 7)
            {
                var drawn = host.DrawCards(1).Single();
                if (drawn.Type == CardType.Action)
                {
                    var shouldSkip = await host.YesNo("Skip drawing Action?", $@"<spans>
                        <run>You may skip</run>
                        <card>{drawn.Name}</card>
                        <run>and put it into your discard pile</run>
                    </spans>");
                    
                    if (shouldSkip)
                    {
                        setAside.Add(drawn.Name);
                    }
                }
            }

            if (setAside.Any())
            {
                host.Discard(setAside.ToArray());
            }
        }
    }
}