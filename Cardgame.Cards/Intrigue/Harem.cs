using Cardgame.API;

namespace Cardgame.Cards.Intrigue
{
    public class Harem : TreasureCardBase, IVictoryCard
    {
        public override CardType[] Types => new[] { CardType.Treasure, CardType.Victory };
        public override string Art => "int-harem";
        public override Cost Cost => 6;

        public override string Text => @"<bold>
            <split>
                <sym large='true'>coin2</sym>
                <sym large='true'>2vp</sym>
            </split>
        </bold>";

        public override Cost Value => 2;

        public int Score(string[] dominion) => 2;
    }
}