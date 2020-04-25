using Cardgame.API;

namespace Cardgame.Cards.Seaside
{
    public class MerchantShip : DurationCardBase
    {
        public override Cost Cost => 5;

        public override string Text => @"<spans>
            <run>Now and at the start of your next turn:</run>
            <sym prefix='+' suffix='.'>coin2</sym>
        </spans>";

        protected override void Act(IActionHost host)
        {
            host.AddCoins(2);
        }

        protected override void OnBeginTurn(IActionHost host)
        {
            host.AddCoins(2);
        }
    }
}