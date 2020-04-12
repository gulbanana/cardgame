namespace Cardgame.Cards
{
    public class Market : KingdomCardModel
    {
        public override string Art => "dom-market";
        
        public override TextModel Text => TextModel.Parse(@"
<block>
    <lines>
        <run>+1 Card</run>
        <run>+1 Action</run>
        <run>+1 Buy</run>
        <spans><run>+</run><sym>coin1</sym></spans>
    </lines>
</block>");    
    }
}