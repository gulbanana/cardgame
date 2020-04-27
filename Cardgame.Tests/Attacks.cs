using System.Threading.Tasks;
using Cardgame.Model;
using Xunit;

namespace Cardgame.Tests
{
    public class Attacks : TestBase
    {
        [Fact]
        public void Militia_ForcesDiscards()
        {
            AddCard("Militia");
            engine.ExecuteChecked(us, new PlayCardCommand { Id = "Militia" });

            Assert.Equal(them, model.ChoosingPlayers.Peek());

            engine.ExecuteChecked(them, new EnterChoiceCommand { Output = "[\"Copper\", \"Copper\"]" });
            
            Assert.Equal(3, model.Hands[them].Count);
        }

        [Fact]
        public async Task Moat_OptionallyPreventsAttacks()
        {
            AddCard("Moat", them); 
            AddCard("Militia", us);
            var militia = engine.ExecuteChecked(us, new PlayCardCommand { Id = "Militia" });
            _ = engine.ExecuteChecked(them, new EnterChoiceCommand { Output = "true" });
            await militia;

            Assert.Equal(6, model.Hands[them].Count);

            AddCard("Militia", us);
            militia = engine.ExecuteChecked(us, new PlayCardCommand { Id = "Militia" });
            _ = engine.ExecuteChecked(them, new EnterChoiceCommand { Output = "false" });
            _ = engine.ExecuteChecked(them, new EnterChoiceCommand { Output = "[\"Copper\", \"Copper\", \"Moat\"]" });
            await militia;

            Assert.Equal(3, model.Hands[them].Count);
        }

        [Fact]
        public void Lighthouse_PreventsEnemyAttacks()
        {
            AddCard("Lighthouse");
            engine.ExecuteChecked(us, new PlayCardCommand { Id = "Lighthouse" } );
            engine.ExecuteChecked(us, new EndTurnCommand { });

            AddCard("Militia");
            engine.ExecuteChecked(us, new PlayCardCommand { Id = "Militia" });

            Assert.Equal(5, model.Hands[them].Count);
        }

        [Fact]
        public void Lighthouse_IgnoresOwnAttacks()
        {
            AddCard("Lighthouse");
            AddCard("Militia");
            model.ActionsRemaining = 2;

            engine.ExecuteChecked(us, new PlayCardCommand { Id = "Lighthouse" });
            engine.ExecuteChecked(us, new PlayCardCommand { Id = "Militia" });

            Assert.DoesNotContain(model.PreventedAttacks, e => e == us);
        }
    }
}
