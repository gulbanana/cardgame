namespace Cardgame.Cards
{
    public class Workshop : ActionCardModel
    {
        public override string Art => "dom-workshop";
        public override int Cost => 3;
        
        public override TextModel Text => TextModel.Parse(@"
        <spans>
            <run>Gain a card costing up to</run>
            <sym>coin4</sym>
            <run>.</run>
        </spans>");
    }
}