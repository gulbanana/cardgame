using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Seaside
{
    public class PirateShip : AttackCardBase
    {
        public override int Cost => 4;
        public override string HasMat => "PirateShipMat";

        public override string Text => @"<small>
            <run>Choose one:</run>
            <sym prefix='+'>coin1</sym>
            <run>per Coin token on your Pirate Ship mat; or each other player reveals the top 2 cards of their deck, trashes one of those Treasures that you choose, and discards the rest, and then if anyone trashed a Treasure you add a Coin token to your Pirate Ship mat.</run>
        </small>";

        protected override async Task ActAsync(IActionHost host)
        {
            var tokens = host.Count(Zone.PlayerMat("PirateShipMat"));

            await host.ChooseOne("Pirate Ship",
                new NamedOption($"<sym prefix='+'>coin1</sym><run>per Coin token (+${tokens}).</run>", () =>
                {                    
                    host.AddCoins(tokens);
                }),
                new NamedOption("Raid for Treasures!", async () =>
                {
                    var anyTrashes = false;
                    await host.Attack(async target =>
                    {
                        var top2 = target.Reveal(Zone.DeckTop(2));
                        var top2Treasures = top2.OfType<ITreasureCard>();
                        if (top2Treasures.Count() == 1)
                        {
                            target.Trash(top2Treasures.Single(), Zone.Deck);
                            anyTrashes = true;
                        }
                        else if (top2Treasures.Count() == 2)
                        {
                            var trashed = await host.SelectCard($"Choose a Treasure for {target.Player} to trash.", top2Treasures);
                            target.Trash(trashed, Zone.Deck);
                            anyTrashes = true;
                        }
                        target.Discard(top2.Except(top2Treasures).ToArray(), Zone.Deck);
                    });

                    if (anyTrashes)
                    {
                        host.PutOnMat("PirateShipMat", "Coin", Zone.Create);
                    }
                })
            );
        }
    }
}