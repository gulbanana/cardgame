namespace Cardgame.Cards
{
    public class Village : ActionCardModel
    {
        public override string Art => "dom-village";
        public override int Cost => 3;
        
        public override TextModel Text => TextModel.Parse(@"
<block>
    <lines>
        <run>+1 Card</run>
        <run>+2 Actions</run>
    </lines>
</block>");
    }
}