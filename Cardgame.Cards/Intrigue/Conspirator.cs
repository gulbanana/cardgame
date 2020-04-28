using Cardgame.API;

namespace Cardgame.Cards.Intrigue
{
    public class Conspirator : ActionCardBase
    {
        public override string Art => "int-conspirator";
        public override Cost Cost => 4;
        
        public override string Text => @"<paras>
            <bold><sym>+coin2</sym></bold>
            <spans>
                <run>If you've played 3 or more Actions this turn (counting this),</run>
                <bold>+1 Card</bold>
                <run>and</run>
                <bold>+1 Action.</bold>
            </spans>
        </paras>";

        protected override void Act(IActionHost host)
        {
            host.AddCoins(2);

            if (host.ActionCount >= 3)
            {
                host.DrawCards(1);
                host.AddActions(1);
            }
        }
    }
}