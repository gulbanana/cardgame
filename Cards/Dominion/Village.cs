namespace Cardgame.Cards
{
    public class Village : KingdomCardModel
    {
        public override string Art => "dom-village";
        
        public override TextModel Text => TextModel.Parse(@"
<block>
    <lines>
        <run>+1 Card</run>
        <run>+2 Actions</run>
    </lines>
</block>");
    }
}