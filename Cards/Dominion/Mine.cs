namespace Cardgame.Cards
{
    public class Mine : KingdomCardModel
    {
        public override string Art => "dom-mine";
        
        public override TextModel Text => TextModel.Parse(@"
<lines>
    <run>Trash a Treasure card from your hand.</run>
    <spans>
        <run>Gain a Treasure card costing up to </run>
        <sym>coin3</sym>
        <run> more; put it into your hand.</run>
    </spans>
</lines>");
    }
}