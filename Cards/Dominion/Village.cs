namespace Cardgame.Cards
{
    public class Village : ActionCardModel
    {
        public override string Art => "dom-village";
        public override int Cost => 3;
        
        public override string Text => @"
        <block>
            <lines>
                <run>+1 Card</run>
                <run>+2 Actions</run>
            </lines>
        </block>";

        protected override void Act(IActionHost host)
        {
            host.DrawCards(1);
            host.AddActions(2);
        }
    }
}