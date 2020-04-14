using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cardgame
{
    public interface IActionHost
    {
        void DrawCards(int n);
        void AddActions(int n);
        void AddBuys(int n);
        void AddMoney(int n);

        void DiscardCards(string[] cards);
        void TrashCards(string[] cards);
        void GainCard(string card);

        Task<T> SelectCard<T>(string prompt, CardSource source, Func<IEnumerable<Cards.CardModel>, IEnumerable<T>> filter) where T : Cards.CardModel;
        Task<string[]> SelectCardsFromHand(string prompt);        
    }

    public static class ActionHostExtensions
    {
        public static void TrashCard(this IActionHost host, string card)
        {
            host.TrashCards(new[] { card });
        }

        public static Task<Cards.CardModel> SelectCard(this IActionHost host, string prompt, CardSource source)
        {
            return host.SelectCard<Cards.CardModel>(prompt, source, x => x);
        }

        public static Task<T> SelectCard<T>(this IActionHost host, string prompt, Func<IEnumerable<Cards.CardModel>, IEnumerable<T>> filter) where T : Cards.CardModel
        {
            return host.SelectCard<T>(prompt, CardSource.Hand, filter);
        }

        public static Task<Cards.CardModel> SelectCard(this IActionHost host, string prompt)
        {
            return host.SelectCard<Cards.CardModel>(prompt, CardSource.Hand, x => x);
        }
    }
}