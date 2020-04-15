using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cardgame
{
    public interface IActionHost
    {
        string Player { get; }
        int ShuffleCount { get; }
        Cards.CardModel[] GetHand();

        Cards.CardModel[] DrawCards(int n);
        void AddActions(int n);
        void AddBuys(int n);
        void AddMoney(int n);

        void Discard(string[] cards, Zone from);
        void Trash(string[] cards, Zone from);
        void Gain(string card, Zone to);        
        void Draw(string name);
        
        Cards.CardModel[] RevealAll(Zone from);
        void RevealAndMove(string card, Zone from, Zone to);

        void PlayAction(Cards.ActionCardModel card, Zone from);

        Task<T[]> SelectCards<T>(string prompt, Zone source, Func<IEnumerable<Cards.CardModel>, IEnumerable<T>> filter, int? min, int? max) where T : Cards.CardModel;
        Task<bool> YesNo(string prompt, string message);

        Task Attack(Func<IActionHost, bool> filter, Func<IActionHost, Task> act);
    }

    public static class ActionHostExtensions
    {
        #region Trash
        public static void Trash(this IActionHost host, string card, Zone from)
        {
            host.Trash(new[] { card }, from);
        }

        public static void Trash(this IActionHost host, Cards.CardModel[] cards, Zone from)
        {
            host.Trash(cards.Select(card => card.Name).ToArray(), from);
        }

        public static void Trash(this IActionHost host, Cards.CardModel card, Zone from)
        {
            host.Trash(new[] { card.Name }, from);
        }

        public static void Trash(this IActionHost host, string card)
        {
            host.Trash(new[] { card }, Zone.Hand);
        }

        public static void Trash(this IActionHost host, Cards.CardModel[] cards)
        {
            host.Trash(cards.Select(card => card.Name).ToArray(), Zone.Hand);
        }

        public static void Trash(this IActionHost host, Cards.CardModel card)
        {
            host.Trash(new[] { card.Name }, Zone.Hand);
        }
        #endregion

        #region Discard
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

        public static void Discard(this IActionHost host, Cards.CardModel[] cards, Zone from)
        {
            host.Discard(cards.Select(card => card.Name).ToArray(), from);
        }

        public static void Discard(this IActionHost host, Cards.CardModel[] cards)
        {
            host.Discard(cards.Select(card => card.Name).ToArray(), Zone.Hand);
        }

        public static void Discard(this IActionHost host, Cards.CardModel card)
        {
            host.Discard(new[]{ card.Name }, Zone.Hand);
        }
        #endregion Discard

        #region Gain
        public static void Gain(this IActionHost host, string card)
        {
            host.Gain(card, Zone.Discard);
        }

        public static void Gain(this IActionHost host, Cards.CardModel card, Zone to)
        {
            host.Gain(card.Name, to);
        }

        public static void Gain(this IActionHost host, Cards.CardModel card)
        {
            host.Gain(card.Name, Zone.Discard);
        }
        #endregion

        public static void PlayAction(this IActionHost host, Cards.ActionCardModel card)
        {
            host.PlayAction(card, Zone.Hand);
        }

        #region SelectCards
        // no numbers
        public static Task<T[]> SelectCards<T>(this IActionHost host, string prompt, Zone source, Func<IEnumerable<Cards.CardModel>, IEnumerable<T>> filter) where T : Cards.CardModel
        {
            return host.SelectCards(prompt, source, filter, null, null);
        }

        public static async Task<T> SelectCard<T>(this IActionHost host, string prompt, Zone source, Func<IEnumerable<Cards.CardModel>, IEnumerable<T>> filter) where T : Cards.CardModel
        {
            var cards = await host.SelectCards(prompt, source, filter, 1, 1);
            return cards.Single();
        }

        // no filter
        public static Task<Cards.CardModel[]> SelectCards(this IActionHost host, string prompt, Zone source)
        {
            return host.SelectCards<Cards.CardModel>(prompt, source, x => x, null, null);
        }

        public static Task<Cards.CardModel[]> SelectCards(this IActionHost host, string prompt, Zone source, int min, int max)
        {
            return host.SelectCards<Cards.CardModel>(prompt, source, x => x, min, max);
        }

        public static async Task<Cards.CardModel> SelectCard(this IActionHost host, string prompt, Zone source)
        {
            var cards = await host.SelectCards<Cards.CardModel>(prompt, source, x => x, 1, 1);
            return cards.Single();
        }

        // no zone
        public static Task<T[]> SelectCards<T>(this IActionHost host, string prompt, Func<IEnumerable<Cards.CardModel>, IEnumerable<T>> filter) where T : Cards.CardModel
        {
            return host.SelectCards<T>(prompt, Zone.Hand, filter, null, null);
        }

        public static Task<T[]> SelectCards<T>(this IActionHost host, string prompt, Func<IEnumerable<Cards.CardModel>, IEnumerable<T>> filter, int min, int max) where T : Cards.CardModel
        {
            return host.SelectCards<T>(prompt, Zone.Hand, filter, min, max);
        }

        public static async Task<T> SelectCard<T>(this IActionHost host, string prompt, Func<IEnumerable<Cards.CardModel>, IEnumerable<T>> filter) where T : Cards.CardModel
        {
            var cards = await host.SelectCards<T>(prompt, Zone.Hand, filter, 1, 1);
            return cards.Single();
        }

        // no filter or zone
        public static Task<Cards.CardModel[]> SelectCards(this IActionHost host, string prompt)
        {
            return host.SelectCards<Cards.CardModel>(prompt, Zone.Hand, x => x, null, null);
        }

        public static Task<Cards.CardModel[]> SelectCards(this IActionHost host, string prompt, int min, int max)
        {
            return host.SelectCards<Cards.CardModel>(prompt, Zone.Hand, x => x, min, max);
        }

        public static async Task<Cards.CardModel> SelectCard(this IActionHost host, string prompt)
        {
            var cards = await host.SelectCards<Cards.CardModel>(prompt, Zone.Hand, x => x, 1, 1);
            return cards.Single();
        }
        #endregion

        public static Task Attack(this IActionHost host, Func<IActionHost, Task> act)
        {
            return host.Attack(_ => true, act);
        }
    }
}