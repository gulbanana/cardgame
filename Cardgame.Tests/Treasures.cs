using Cardgame.Model;
using Xunit;

namespace Cardgame.Tests
{
    public class Treasures : TestBase
    {
        [Fact]
        public void OneByOne()
        {
            engine.ExecuteChecked(us, new PlayCardCommand { Id = "Copper" });
            Assert.Equal(1, engine.Model.CoinsRemaining);

            AddCard("Silver");
            engine.ExecuteChecked(us, new PlayCardCommand { Id = "Silver" });
            Assert.Equal(3, engine.Model.CoinsRemaining);

            AddCard("Gold");
            engine.ExecuteChecked(us, new PlayCardCommand { Id = "Gold" });
            Assert.Equal(6, engine.Model.CoinsRemaining);
        }

        [Fact]
        public void CombinedPlay()
        {
            model.Hands[us].Clear();
            model.Hands[us].Add(Instance.Of("Copper"));
            model.Hands[us].Add(Instance.Of("Silver"));
            model.Hands[us].Add(Instance.Of("Gold"));

            engine.ExecuteChecked(us, new PlayAllTreasuresCommand { });
            Assert.Equal(6, engine.Model.CoinsRemaining);
        }


        [Theory, InlineData(5), InlineData(9), InlineData(10), InlineData(24), InlineData(25)]
        public void PhilosophersStone(int deckSize)
        {
            while (model.Decks[us].Count < deckSize)
            {
                model.Decks[us].Add(Instance.Of("Estate"));
            }

            AddCard(nameof(Cards.Alchemy.Philosopher_sStone));
            engine.ExecuteChecked(us, new PlayCardCommand { Id = nameof(Cards.Alchemy.Philosopher_sStone) });
            Assert.Equal(deckSize / 5, engine.Model.CoinsRemaining);
        }
    }
}
