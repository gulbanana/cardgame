using System.Threading.Tasks;

namespace Cardgame
{
    public partial class GameEngine : IActionHost
    {
        void IActionHost.DrawCards(int n)
        {
            var reshuffled = false;
            for (var i = 0; i < n; i++)
            {
                reshuffled = reshuffled | DrawCard(Model.ActivePlayer);
            }
            if (reshuffled)
            {
                LogPartialEvent($@"<spans>
                    <run>...</run>
                    <run>(</run>
                    <if you='you reshuffle.' them='reshuffling.'>{Model.ActivePlayer}</if>
                    <run>)</run>
                </spans>");
            }

            LogPartialEvent($@"<spans>
                <run>...</run>
                <if you='you draw' them='drawing'>{Model.ActivePlayer}</if>
                <run>{n} cards.</run>
            </spans>");
        }

        void IActionHost.AddActions(int n)
        {
            Model.ActionsRemaining += n;

            LogPartialEvent($@"<spans>
                <run>...</run>
                <if you='you get' them='getting'>{Model.ActivePlayer}</if>
                <run>+{n} actions.</run>
            </spans>");
        }

        void IActionHost.AddBuys(int n)
        {
            Model.BuysRemaining += n;

            LogPartialEvent($@"<spans>
                <run>...</run>
                <if you='you get' them='getting'>{Model.ActivePlayer}</if>
                <run>+{n} buys.</run>
            </spans>");
        }

        void IActionHost.AddMoney(int n)
        {
            Model.MoneyRemaining += n;

            LogPartialEvent($@"<spans>
                <run>...</run>
                <if you='you get' them='getting'>{Model.ActivePlayer}</if>
                <run>+${n}.</run>
            </spans>");
        }

        void IActionHost.DiscardCards(string[] cards)
        {
            foreach (var card in cards)
            {
                DiscardCard(Model.ActivePlayer, card);
            }

            LogPartialEvent($@"<spans>
                <run>...</run>
                <if you='you discard' them='discarding'>{Model.ActivePlayer}</if>
                <run>{cards.Length} cards.</run>
            </spans>");
        }

        Task<string[]> IActionHost.SelectCardsFromHand(string prompt)
        {
            return Choose<string[], string[]>(
                ChoiceType.SelectCards, 
                $"<run>{prompt}</run>", 
                Model.Hands[Model.ActivePlayer].ToArray()
            );
        }
    }
}