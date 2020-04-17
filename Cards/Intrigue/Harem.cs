using Cardgame.API;

namespace Cardgame.Cards.Intrigue
{
    public class Harem : CardBase, ITreasureCard, IVictoryCard
    {
        public override CardType[] Types => new[] { CardType.Treasure, CardType.Victory };
        public override string Art => "int-harem";
        public override int Cost => 6;

        public override string Text => @"<split>
            <sym large='true'>coin2</sym>
            <sym large='true' prefix='2'>vp</sym>
        </split>";

        public int Value => 2;

        public int Score(string[] dominion) => 2;
    }
}