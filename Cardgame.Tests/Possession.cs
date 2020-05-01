using System;
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
    }
}
