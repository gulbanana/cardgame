using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Intrigue
{
    public class Lurker : ActionCardBase
    {
        public override string Art => "int-lurker";
        public override Cost Cost => 2;

        public override string Text => @"<paras>
            <bold>+1 Action</bold>
            <run>Choose one: Trash an Action card from the Supply; or gain an Action card from the trash.</run>
        </paras>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.AddActions(1);
            
            await host.ChooseOne("Lurker", 
                new NamedOption("Trash an Action card from the Supply", async () =>
                {
                    var trashed = await host.SelectCard("Choose a card to trash.", Zone.SupplyAvailable, cards => cards.OfType<IActionCard>());
                    host.Trash(trashed, Zone.SupplyAvailable);
                }),
                new NamedOption("Gain an Action card from the Trash", async () => 
                {
                    var gained = await host.SelectCard("Choose a card to gain.", Zone.Trash, cards => cards.OfType<IActionCard>());
                    await host.GainFrom(gained, Zone.Trash);
                })
            );
        }
    }
}