namespace Cardgame.Cards
{
    public class Militia : ActionCardModel
    {
        public override string SubType => "Attack";
        public override string Art => "dom-militia";
        public override int Cost => 4;        

        public override TextModel Text => TextModel.Parse(@"
        <paras>
            <block>
                <spans>
                    <run>+</run>
                    <sym>coin2</sym>
                </spans>
            </block>
            <run>Each other player discards down to 3 cards in his hand.</run>
        </paras>");
    }
}