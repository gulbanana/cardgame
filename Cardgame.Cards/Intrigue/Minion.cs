using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Intrigue
{
    public class Minion : AttackCardBase
    {
        public override string Art => "int-minion";
        public override Cost Cost => 5;

        public override string Text => @"<paras>
            <bold>+1 Action</bold>
            <spans>
                <run>Choose one:</run>
                <sym prefix='+' suffix=';'>coin2</sym>
                <run>or discard your hand,</run>
                <bold>+4 Cards,</bold>
                <run>and each other player with at least 5 cards in hand discards their hand and draws 4 cards.</run>
            </spans>
        </paras>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.AddActions(1);

            await host.ChooseOne("Minion",
                new NamedOption("<sym prefix='+'>coin2</sym>", () => host.AddCoins(2)),
                new NamedOption("<run>More</run><card suffix='s!'>Minion</card>", async () => 
                {
                    host.Discard(Zone.Hand);
                    host.DrawCards(4);
                    await host.Attack(target => target.Count(Zone.Hand) >= 5, target => 
                    {
                        target.Discard(Zone.Hand);
                        target.DrawCards(4);
                    });
                })
            );
        }
    }
}