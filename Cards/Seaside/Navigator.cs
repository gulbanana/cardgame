using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Seaside
{
    public class Navigator : ActionCardBase
    {
        public override int Cost => 4;

        public override string Text => @"<paras>
            <sym prefix='+'>coin2</sym>
            <run>Look at the top 5 cards of your deck. Either discard them all, or put them back in any order.</run>
        </paras>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.AddCoins(2);
            
            var top5 = host.Examine(Zone.DeckTop4);
            
            var namesBuilder = new StringBuilder();
            {
                for (var i = 0; i < top5.Length; i++)
                {
                    var suffix = (i == top5.Length - 1) ? "" :
                        (i == top5.Length - 2) ? " and" :
                        ",";

                    namesBuilder.Append($"<card suffix='{suffix}'>{top5[i].Name}</card>");
                }
            }
            var names = namesBuilder.ToString();

            await host.ChooseOne("Navigator",
                 new NamedOption($"<run>Discard cards:</run>{names}.", () => host.Discard(top5, Zone.DeckTop4)),
                 new NamedOption("<run>Put cards back on deck.</run>", async () => 
                 {
                     var reordered = await host.OrderCards("Navigator", Zone.DeckTop4);
                     host.Reorder(reordered, Zone.DeckTop4);
                 })
            );
        }
    }
}