using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Intrigue
{
    public class Masquerade : ActionCardBase
    {
        public override string Art => "int-masquerade";
        public override int Cost => 3;

        public override string Text => @"<paras>
            <bold>+2 Cards</bold>
            <run>Each player with any cards in hand passes one to the next such player to their left, at once. Then you may trash a card fom your hand.</run>
        </paras>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.DrawCards(2);

            var selections = new Dictionary<IActionHost, string>();
            await host.AllPlayers(async player => 
            {
                var selected = await player.SelectCard("Choose a card to pass to your left.", Zone.Hand);
                selections[player] = selected.Name;
            });

            foreach (var kvp in selections)
            {
                kvp.Key.PassCardLeft(kvp.Value);
            }

            var trashed = await host.SelectCards("Choose up to one card to trash.", Zone.Hand, 0, 1);
            if (trashed.Any())
            {
                host.Trash(trashed);
            }
        }
    }
}