namespace Cardgame.Cards
{
    public class Smithy : KingdomCardModel
    {
        public override string Art => "dom-smithy";

        public override TextModel Text => TextModel.Parse(@"
<block>
    <run>+3 Cards</run>
</block>");
    }
}