using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Intrigue
{
    public class Torturer : AttackCardBase
    {
        public override string Art => "int-torturer";
        public override Cost Cost => 5;

        public override string Text => @"<paras>
            <bold>+3 Cards</bold>
            <run>Each other player either discards 2 cards or gains a Curse to their hand, their choice. (They may pick an option they can't do.)</run>
        </paras>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.DrawCards(3);

            await host.Attack(async target =>
            {
                await target.ChooseOne("Torturer",
                    new NamedOption("Discard 2 cards", async () => 
                    {
                        var handSize = target.Count(Zone.Hand);
                        if (handSize > 2)
                        {
                            var discarded = await target.SelectCards("Choose cards to discard.", Zone.Hand, 2, 2);
                            target.Discard(discarded);
                        }
                        else
                        {
                            target.Discard(Zone.Hand);
                        }
                    }),
                    new NamedOption("Gain a Curse", () => target.Gain("Curse", Zone.Hand))
                );
            });
        }
    }
}