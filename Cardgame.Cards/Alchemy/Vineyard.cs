using System.Linq;
using Cardgame.API;

namespace Cardgame.Cards.Alchemy
{
    public class Vineyard : VictoryCardBase
    {
        public override Cost Cost => new Cost(0, true);

        public override string Text => @"
            <run>Worth</run>
            <sym prefix='1'>vp</sym>
            <run>per 3 Action cards you have (round down).</run>
        ";

        public override int Score(string[] dominion) 
        {
            return dominion.Select(AllCards.ByName).Count(card => card.Types.Contains(CardType.Action)) / 3;
        }
    }
}