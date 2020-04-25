using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Seaside
{
    public class Island : VictoryActionCardBase
    {
        public override Cost Cost => 4;
        public override int Score => 2;
        public override string HasMat => "IslandMat";

        public override string Text => @"<split>
            <run>Put this and a card from your hand onto your Island mat.</run>
            <bold><sym large='true' prefix='2'>vp</sym></bold>
        </split>";

        protected override async Task ActAsync(IActionHost host)
        {
            var matted = await host.SelectCard("Choose an additional card to put on your Island mat.");
            host.PutOnMat("IslandMat", "Island", Zone.InPlay);
            host.PutOnMat("IslandMat", matted, Zone.Hand);
        }
    }
}