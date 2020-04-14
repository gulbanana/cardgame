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
                <sym prefix='+'>coin2</sym>
            </block>
            <run>Each other player discards down to 3 cards in his hand.</run>
        </paras>");
    }
}