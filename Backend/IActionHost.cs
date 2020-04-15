using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cardgame
{
    public interface IActionHost
    {
        string Player { get; }
        int ShuffleCount { get; }
        string[] GetHand();

        void DrawCards(int n);
        void AddActions(int n);
        void AddBuys(int n);
        void AddMoney(int n);

        void Discard(string[] cards, Zone from);
        void Trash(string[] cards);
        void Gain(string card, Zone to);
        void Draw(string name);
        Cards.CardModel Reveal();

        Task<T> SelectCard<T>(string prompt, CardSource source, Func<IEnumerable<Cards.CardModel>, IEnumerable<T>> filter) where T : Cards.CardModel;
        Task<string[]> SelectCardsFromHand(string prompt, int? number = null);
        Task<bool> YesNo(string prompt);

        Task Attack(Func<IActionHost, bool> filter, Func<IActionHost, Task> act);
    }

    public static class ActionHostExtensions
    {
        public static void Trash(this IActionHost host, string card)
        {
            host.Trash(new[] { card });
        }

        public static void Discard(this IActionHost host, string card, Zone from)
        {
            host.Discard(new[] { card }, from);
        }

        public static void Discard(this IActionHost host, string[] cards)
        {
            host.Discard(cards, Zone.Hand);
        }

        public static void Discard(this IActionHost host, string card)
        {
            host.Discard(new[] { card }, Zone.Hand);
        }

        public static void Gain(this IActionHost host, string card)
        {
            host.Gain(card, Zone.Discard);
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

        public static Task Attack(this IActionHost host, Func<IActionHost, Task> act)
        {
            return host.Attack(_ => true, act);
        }
    }
}