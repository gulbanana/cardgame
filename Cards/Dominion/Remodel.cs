namespace Cardgame.Cards
{
    public class Remodel : ActionCardModel
    {
        public override string Art => "dom-remodel";
        public override int Cost => 4;
        
        public override TextModel Text => TextModel.Parse(@"
        <lines>
            <run>Trash a card from your hand.</run>
            <spans>
                <run>Gain a card costing up to </run>
                <sym>coin2</sym>
                <run> more than the trashed card.</run>
            </spans>
        </lines>");
    }
}