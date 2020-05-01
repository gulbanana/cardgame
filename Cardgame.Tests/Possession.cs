using System;
using System.Linq;
using System.Threading.Tasks;
using Cardgame.Model;
using Xunit;

namespace Cardgame.Tests
{
    public class Possession : TestBase
    {
        [Fact]
        public async Task InsertsControlledTurn()
        {
            AddCard("Possession");
            await engine.ExecuteChecked(us, new PlayCardCommand { Id = "Possession" });

            Assert.Equal(us, model.ActivePlayer);
            Assert.Equal(us, model.ControllingPlayer);

            await engine.ExecuteChecked(us, new EndTurnCommand());

            Assert.Equal(them, model.ActivePlayer);
            Assert.Equal(us, model.ControllingPlayer);
            await Assert.ThrowsAnyAsync<Exception>(() => engine.ExecuteChecked(them, new PlayCardCommand { Id = "Copper" }));
            await engine.ExecuteChecked(us, new PlayCardCommand { Id = "Copper" });

            await engine.ExecuteChecked(us, new EndTurnCommand());

            Assert.Equal(them, model.ActivePlayer);
            Assert.Equal(them, model.ControllingPlayer);
            await engine.ExecuteChecked(them, new PlayCardCommand { Id = "Copper" });
        }

        [Fact]
        public async Task BuyGainToController()
        {
            AddCard("Possession");
            await engine.ExecuteChecked(us, new PlayCardCommand { Id = "Possession" });
            await engine.ExecuteChecked(us, new EndTurnCommand());

            model.CoinsRemaining = 3;
            await engine.ExecuteChecked(us, new BuyCardCommand { Id = "Silver" });
            
            Assert.False(model.Discards[them].Contains("Silver"));
            Assert.True(model.Discards[us].Contains("Silver"));
        }
    }
}
