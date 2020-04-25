using Cardgame.API;

namespace Cardgame.Cards.Intrigue
{
    public class Coppersmith : ActionCardBase
    {
        public override Cost Cost => 4;
        
        public override string Text => @"<spans>
            <run>Copper produces an extra</run>
            <sym>coin1</sym>
            <run>this turn.</run>
        </spans>";

        protected override void Act(IActionHost host)
        {
            host.AddEffect(nameof(CoppersmithEffect));
        }
    }
}