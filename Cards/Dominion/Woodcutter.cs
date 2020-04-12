namespace Cardgame.Cards
{
    public class Woodcutter : KingdomCardModel
    {
        public override string Art => "dom-woodcutter";

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
    }
}