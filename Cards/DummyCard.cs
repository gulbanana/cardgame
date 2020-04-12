namespace Cardgame.Cards
{
    internal class DummyCard : CardModel
    {
        public override CardType Type => CardType.Action;
        public override string Art => "dom-village";
        public override int Cost => 0;
        public override TextModel Text => TextModel.Parse("<run>Card not found.</run>");

        public DummyCard(string name)
        {

        }
    }
}