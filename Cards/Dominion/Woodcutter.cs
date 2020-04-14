namespace Cardgame.Cards
{
    public class Woodcutter : ActionCardModel
    {
        public override string Art => "dom-woodcutter";
        public override int Cost => 3;

        public override TextModel Text => TextModel.Parse(@"
<block>
    <lines>
        <run>+1 Buy</run>
        <spans>
            <run>+</run>
            <sym>coin2</sym>
        </spans>
    </lines>
</block>");

        protected override void Act(IActionHost host)
        {
            host.AddBuys(1);
            host.AddMoney(2);
        }
    }
}