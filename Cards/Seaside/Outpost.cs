using System.Linq;
using Cardgame.API;

namespace Cardgame.Cards.Seaside
{
    public class Outpost : ModifierDurationCardBase, IModifier
    {
        public override Cost Cost => 5;

        public override string Text => @"
            If this is the first time you played an Outpost this turn, and the previous turn wasn't yours, 
            then take an extra turn after this one, and you only draw 3 cards for your next hand.
        ";

        protected override void Act(IActionHost host)
        {
            if (host.Examine(Zone.RecentPlays).Any(card => card.Name == "Outpost") || host.PreviousPlayer == host.Player)
            {
                host.CompleteDuration();
            }
        }

        public override bool TakeAnotherTurn => true;        
        public override int? NextHandSize => 3;
    }
}