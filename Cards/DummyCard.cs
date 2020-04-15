namespace Cardgame.Cards
{
    internal class DummyCard : CardModel
    {
        public override CardType Type => CardType.Victory;
        public override string Art => "dom-village";
        public override int Cost => 0;
        public override string Text => "<run>Card not found.</run>";

        public DummyCard(string name)
        {
            Name = name;
        }
    }
}