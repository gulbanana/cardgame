namespace Cardgame.Cards
{
    public class Moat : KingdomCardModel
    {
        public override string Art => "dom-moat";

        public override string SubType => "Reaction";

        public override TextModel Text => TextModel.Parse(@"
<paras>
    <block>
        <run>+2 Cards</run>
    </block>
    <run>When another player plays an Attack card, you may reveal this from your hand. If you do, you are unaffected by that attack.</run>
</paras>");
    }
}