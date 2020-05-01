using System.Threading.Tasks;
using Cardgame.Model;
using Xunit;

namespace Cardgame.Tests
{
    public class TurnSequence : TestBase
    {
        [Fact]
        public async Task RotateTurns()
        {
            model.ActivePlayer = "player1";
            
            await engine.EndTurnAsync(model.ActivePlayer);
            Assert.Equal("player2", model.ActivePlayer);
            Assert.Equal("player2", model.ControllingPlayer);

            await engine.EndTurnAsync(model.ActivePlayer);
            Assert.Equal("player1", model.ActivePlayer);
            Assert.Equal("player1", model.ControllingPlayer);
        }

        [Fact]
        public async Task ExtraTurn()
        {
            model.ActivePlayer = "player1";            
            engine.ExtraTurns.Enqueue(("player1", "player1"));

            await engine.EndTurnAsync(model.ActivePlayer);
            Assert.Equal("player1", model.ActivePlayer);
            Assert.Equal("player1", model.ControllingPlayer);

            await engine.EndTurnAsync(model.ActivePlayer);
            Assert.Equal("player2", model.ActivePlayer);
            Assert.Equal("player2", model.ControllingPlayer);
        }

        [Fact]
        public async Task ExtraTurnForAnotherPlayer()
        {
            model.ActivePlayer = "player1";
            
            engine.ExtraTurns.Enqueue(("player2", "player2"));

            await engine.EndTurnAsync(model.ActivePlayer);
            Assert.Equal("player2", model.ActivePlayer);
            Assert.Equal("player2", model.ControllingPlayer);

            await engine.EndTurnAsync(model.ActivePlayer);
            Assert.Equal("player2", model.ActivePlayer);
            Assert.Equal("player2", model.ControllingPlayer);
        }

        [Fact]
        public async Task ExtraTurnWithController()
        {
            model.ActivePlayer = "player1";
            
            engine.ExtraTurns.Enqueue(("player2", "player1"));

            await engine.EndTurnAsync(model.ActivePlayer);
            Assert.Equal("player2", model.ActivePlayer);
            Assert.Equal("player1", model.ControllingPlayer);

            await engine.EndTurnAsync(model.ActivePlayer);
            Assert.Equal("player2", model.ActivePlayer);
            Assert.Equal("player2", model.ControllingPlayer);
        }

        [Fact]
        public async Task StackedExtraTurns()
        {
            model.ActivePlayer = "player1";            
            engine.ExtraTurns.Enqueue(("player1", "player1"));
            engine.ExtraTurns.Enqueue(("player1", "player1"));
            engine.ExtraTurns.Enqueue(("player1", "player1"));

            await engine.EndTurnAsync(model.ActivePlayer);
            Assert.Equal("player1", model.ActivePlayer);
            Assert.Equal("player1", model.ControllingPlayer);

            await engine.EndTurnAsync(model.ActivePlayer);
            Assert.Equal("player1", model.ActivePlayer);
            Assert.Equal("player1", model.ControllingPlayer);

            await engine.EndTurnAsync(model.ActivePlayer);
            Assert.Equal("player1", model.ActivePlayer);
            Assert.Equal("player1", model.ControllingPlayer);

            await engine.EndTurnAsync(model.ActivePlayer);
            Assert.Equal("player2", model.ActivePlayer);
            Assert.Equal("player2", model.ControllingPlayer);
        }

        [Fact]
        public async Task RepeatedExtraTurns()
        {
            model.ActivePlayer = "player1";            
            engine.ExtraTurns.Enqueue(("player1", "player1"));

            await engine.EndTurnAsync(model.ActivePlayer);
            Assert.Equal("player1", model.ActivePlayer);
            Assert.Equal("player1", model.ControllingPlayer);

            engine.ExtraTurns.Enqueue(("player1", "player1"));
            await engine.EndTurnAsync(model.ActivePlayer);
            Assert.Equal("player1", model.ActivePlayer);
            Assert.Equal("player1", model.ControllingPlayer);

            engine.ExtraTurns.Enqueue(("player1", "player1"));
            await engine.EndTurnAsync(model.ActivePlayer);
            Assert.Equal("player1", model.ActivePlayer);
            Assert.Equal("player1", model.ControllingPlayer);

            await engine.EndTurnAsync(model.ActivePlayer);
            Assert.Equal("player2", model.ActivePlayer);
            Assert.Equal("player2", model.ControllingPlayer);
        }
    }
}
