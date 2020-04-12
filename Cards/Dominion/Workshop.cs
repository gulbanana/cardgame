namespace Cardgame.Cards
{
    public class Workshop : KingdomCardModel
    {
        public override string Art => "dom-workshop";

        public override TextModel Text => TextModel.Parse(@"
<spans>
    <run>Gain a card costing up to</run>
    <sym>coin4</sym>
    <run>.</run>
</spans>");
    }
}