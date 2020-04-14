using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cardgame
{
    public partial class GameEngine : IActionHost
    {
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
                {(n == 1 ? "a card." : "<run>{n} cards.</run>")}
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
                <if you='you trash' them='trashing'>{Model.ActivePlayer}</if>
                {(cards.Length == 1 ? "a card." : "<run>{cards.Length} cards.</run>")}
            </spans>");
        }

        void IActionHost.TrashCards(string[] cards)
        {
            foreach (var card in cards)
            {
                TrashCard(Model.ActivePlayer, card);
            }

            LogPartialEvent($@"<spans>
                <run>...</run>
                <if you='you trash' them='trashing'>{Model.ActivePlayer}</if>
                <run>{(cards.Length == 1 ? "a card." : "{cards.Length} cards.")}</run>
            </spans>");
        }

        void IActionHost.GainCard(string id)
        {
            GainCard(Model.ActivePlayer, id);

            LogPartialEvent($@"<spans>
                <run>...</run>
                <if you='you gain' them='gaining'>{Model.ActivePlayer}</if>
                <card>{id}</card>
                <run>.</run>
            </spans>");
        }

        async Task<T> IActionHost.SelectCard<T>(string prompt, CardSource source, Func<IEnumerable<Cards.CardModel>, IEnumerable<T>> filter)
        {
            var sourceCards = source switch 
            {
                CardSource.Hand => Model.Hands[Model.ActivePlayer],
                CardSource.Kingdom => Model.KingdomCards.Concat(new[]{"Estate", "Duchy", "Province", "Copper", "Silver", "Gold"}).Where(id => Model.CardStacks[id] > 0)
            };

            var filteredCards = filter(sourceCards.Select(id => Cards.All.ByName[id]));

            var id = await Choose<string[], string>(
                ChoiceType.SelectCard, 
                $"<run>{prompt}</run>", 
                filteredCards.Select(card => card.Name).ToArray()
            );

            return filteredCards.First(card => card.Name == id);
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