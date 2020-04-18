using Cardgame.API;

namespace Cardgame.Cards.Intrigue
{
    public class Conspirator : ActionCardBase
    {
        public override string Art => "int-conspirator";
        public override int Cost => 4;
        
        public override string Text => @"<paras>
            <sym prefix='+'>coin2</sym>
            <spans>
                <run>If you've played 3 or more Actions this turn (counting this),</run>
                <block><run>+1 Card</run></block>
                <run>and</run>
                <block><run>+1 Action.</run></block>
            </spans>
        </paras>";

        protected override void Act(IActionHost host)
        {
            host.AddMoney(2);

            if (host.ActionCount >= 3)
            {
                host.DrawCards(1);
                host.AddActions(1);
            }
        }
    }
}