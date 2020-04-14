namespace Cardgame.Cards
{
    public class Market : ActionCardModel
    {
        public override string Art => "dom-market";
        public override int Cost => 5;

        public override TextModel Text => TextModel.Parse(@"
<block>
    <lines>
        <run>+1 Card</run>
        <run>+1 Action</run>
        <run>+1 Buy</run>
        <spans><run>+</run><sym>coin1</sym></spans>
    </lines>
</block>");    

        protected override void Act(IActionHost host)
        {
            host.DrawCards(1);
            host.AddActions(1);
            host.AddBuys(1);
            host.AddMoney(1);
        }
    }
}