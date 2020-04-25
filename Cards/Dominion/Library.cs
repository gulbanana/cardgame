using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class Library : ActionCardBase
    {
        public override string Art => "dom-library";
        public override Cost Cost => 5;
        
        public override string Text => @"Draw until you have 7 cards in hand, skipping any Action cards you choose to; set those aside, discarding them afterwards.";

        protected override async Task ActAsync(IActionHost host)
        {
            var setAside = new List<string>();

            while (host.Count(Zone.Hand) < 7)
            {
                var drawn = host.DrawCards(1).SingleOrDefault();
                if (drawn == null) break; // no cards left in deck or discard

                if (drawn.Types.Contains(CardType.Action))
                {
                    var shouldSkip = await host.YesNo("Library", $@"<spans>
                        <run>Skip drawing</run>
                        <card>{drawn.Name}</card>
                        <run>and put it into your discard pile?</run>
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