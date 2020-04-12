namespace Cardgame.Cards
{
    public class Cellar : KingdomCardModel
    {
        public override string Art => "dom-cellar";
        public override int Cost => 2;
        
        public override TextModel Text => TextModel.Parse(@"
<paras>
    <block>
        <run>+1 Action</run>
    </block>
    <lines>
        <run>Discard any number of cards.</run>
        <run>+1 Card per card discarded.</run>
    </lines>
</paras>");
    }
}