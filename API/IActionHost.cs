using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cardgame.Shared;

namespace Cardgame.API
{
    public interface IActionHost
    {
        string Player { get; }
        int ShuffleCount { get; }
        
        // the four basic operations
        ICard[] DrawCards(int n);
        void AddActions(int n);
        void AddBuys(int n);
        void AddMoney(int n);
        
        // move cards around
        void Discard(string[] cards, Zone from);
        void Trash(string[] cards, Zone from);
        void Gain(string card, Zone to);
        void GainFrom(string[] cards, Zone from);
        void PlaceOnDeck(string card, Zone from);
        void Reveal(string[] cards, Zone from);

        // manipulate entire zones
        ICard[] Examine(Zone @in);
        void Reorder(string[] cards, Zone @in);

        // special cases
        void DiscardEntireDeck();

        // potentially-interactive actions
        Task PlayCard(string card, Zone from);
        Task Attack(Func<IActionHost, bool> filter, Func<IActionHost, Task> act, bool benign = false);

        // user inputs
        Task<bool> YesNo(string prompt, string message);
        Task ChooseOne(string prompt, string option1, Action action1, string option2, Action action2);
        Task<T[]> SelectCards<T>(string prompt, Zone source, Func<IEnumerable<ICard>, IEnumerable<T>> filter, int? min, int? max) where T : ICard;
        Task<ICard[]> OrderCards(string prompt, Zone source);

        // advanced
        IModifier[] GetModifiers();
        void AddEffect(string effect);
        void RemoveEffect(string effect);
    }

    public static class ActionHostExtensions
    {
        #region Trash
        public static void Trash(this IActionHost host, string card, Zone from)
        {
            host.Trash(new[] { card }, from);
        }

        public static void Trash(this IActionHost host, ICard[] cards, Zone from)
        {
            host.Trash(cards.Select(card => card.Name).ToArray(), from);
        }

        public static void Trash(this IActionHost host, ICard card, Zone from)
        {
            host.Trash(new[] { card.Name }, from);
        }

        public static void Trash(this IActionHost host, string card)
        {
            host.Trash(new[] { card }, Zone.Hand);
        }

        public static void Trash(this IActionHost host, ICard[] cards)
        {
            host.Trash(cards.Select(card => card.Name).ToArray(), Zone.Hand);
        }

        public static void Trash(this IActionHost host, ICard card)
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

        public static void Discard(this IActionHost host, ICard[] cards, Zone from)
        {
            host.Discard(cards.Select(card => card.Name).ToArray(), from);
        }

        public static void Discard(this IActionHost host, ICard card, Zone from)
        {
            host.Discard(card.Name, from);
        }

        public static void Discard(this IActionHost host, ICard[] cards)
        {
            host.Discard(cards.Select(card => card.Name).ToArray(), Zone.Hand);
        }

        public static void Discard(this IActionHost host, ICard card)
        {
            host.Discard(new[]{ card.Name }, Zone.Hand);
        }
        #endregion Discard

        #region Gain
        public static void Gain(this IActionHost host, string card)
        {
            host.Gain(card, Zone.Discard);
        }

        public static void Gain(this IActionHost host, ICard card, Zone to)
        {
            host.Gain(card.Name, to);
        }

        public static void Gain(this IActionHost host, ICard card)
        {
            host.Gain(card.Name, Zone.Discard);
        }

        public static void GainFrom(this IActionHost host, ICard[] cards, Zone from)
        {
            host.GainFrom(cards.Select(card => card.Name).ToArray(), from);
        }
        #endregion

        #region Place
        public static void PlaceOnDeck(this IActionHost host, ICard card, Zone from)
        {
            host.PlaceOnDeck(card.Name, from);
        }
        #endregion

        #region Reveal
        public static void Reveal(this IActionHost host, string card, Zone from)
        {
            host.Reveal(new[] { card }, from);
        }

        public static void Reveal(this IActionHost host, string card)
        {
            host.Reveal(new[] { card }, Zone.Hand);
        }
        
        public static void Reveal(this IActionHost host, ICard[] cards, Zone from)
        {
            host.Reveal(cards.Select(card => card.Name).ToArray(), from);
        }

        public static void Reveal(this IActionHost host, ICard[] cards)
        {
            host.Reveal(cards.Select(card => card.Name).ToArray(), Zone.Hand);
        }

        public static void Reveal(this IActionHost host, ICard card, Zone from)
        {
            host.Reveal(new[] { card.Name }, from);
        }

        public static void Reveal(this IActionHost host, ICard card)
        {
            host.Reveal(new[] { card.Name }, Zone.Hand);
        }
        #endregion

        public static void Reorder(this IActionHost host, ICard[] cards, Zone @in)
        {
            host.Reorder(cards.Select(card => card.Name).ToArray(), @in);
        }

        public static int Count(this IActionHost host, Zone @in)
        {
            return host.Examine(@in).Count();
        }

        public static void PlayCard(this IActionHost host, string card)
        {
            host.PlayCard(card, Zone.Hand);
        }

        public static void PlayCard(this IActionHost host, IActionCard card, Zone from)
        {
            host.PlayCard(card.Name, from);
        }

        public static void PlayCard(this IActionHost host, IActionCard card)
        {
            host.PlayCard(card.Name, Zone.Hand);
        }

        #region SelectCards
        // no numbers
        public static Task<T[]> SelectCards<T>(this IActionHost host, string prompt, Zone source, Func<IEnumerable<ICard>, IEnumerable<T>> filter) where T : ICard
        {
            return host.SelectCards(prompt, source, filter, null, null);
        }

        public static async Task<T> SelectCard<T>(this IActionHost host, string prompt, Zone source, Func<IEnumerable<ICard>, IEnumerable<T>> filter) where T : ICard
        {
            var cards = await host.SelectCards(prompt, source, filter, 1, 1);
            return cards.SingleOrDefault();
        }

        // no filter
        public static Task<ICard[]> SelectCards(this IActionHost host, string prompt, Zone source)
        {
            return host.SelectCards<ICard>(prompt, source, x => x, null, null);
        }

        public static Task<ICard[]> SelectCards(this IActionHost host, string prompt, Zone source, int min, int max)
        {
            return host.SelectCards<ICard>(prompt, source, x => x, min, max);
        }

        public static async Task<ICard> SelectCard(this IActionHost host, string prompt, Zone source)
        {
            var cards = await host.SelectCards<ICard>(prompt, source, x => x, 1, 1);
            return cards.SingleOrDefault();
        }

        // no zone
        public static Task<T[]> SelectCards<T>(this IActionHost host, string prompt, Func<IEnumerable<ICard>, IEnumerable<T>> filter) where T : ICard
        {
            return host.SelectCards<T>(prompt, Zone.Hand, filter, null, null);
        }

        public static Task<T[]> SelectCards<T>(this IActionHost host, string prompt, IEnumerable<T> choices) where T : ICard
        {
            return host.SelectCards<T>(prompt, Zone.Hand, _ => choices, null, null);
        }

        public static Task<T[]> SelectCards<T>(this IActionHost host, string prompt, Func<IEnumerable<ICard>, IEnumerable<T>> filter, int min, int max) where T : ICard
        {
            return host.SelectCards<T>(prompt, Zone.Hand, filter, min, max);
        }

        public static Task<T[]> SelectCards<T>(this IActionHost host, string prompt, IEnumerable<T> choices, int min, int max) where T : ICard
        {
            return host.SelectCards<T>(prompt, Zone.Hand, _ => choices, min, max);
        }

        public static async Task<T> SelectCard<T>(this IActionHost host, string prompt, Func<IEnumerable<ICard>, IEnumerable<T>> filter) where T : ICard
        {
            var cards = await host.SelectCards<T>(prompt, Zone.Hand, filter, 1, 1);
            return cards.SingleOrDefault();
        }

        public static async Task<T> SelectCard<T>(this IActionHost host, string prompt, IEnumerable<T> choices) where T : ICard
        {
            var cards = await host.SelectCards<T>(prompt, Zone.Hand, _ => choices, 1, 1);
            return cards.SingleOrDefault();
        }

        // no filter or zone
        public static Task<ICard[]> SelectCards(this IActionHost host, string prompt)
        {
            return host.SelectCards<ICard>(prompt, Zone.Hand, x => x, null, null);
        }

        public static Task<ICard[]> SelectCards(this IActionHost host, string prompt, int min, int max)
        {
            return host.SelectCards<ICard>(prompt, Zone.Hand, x => x, min, max);
        }

        public static async Task<ICard> SelectCard(this IActionHost host, string prompt)
        {
            var cards = await host.SelectCards<ICard>(prompt, Zone.Hand, x => x, 1, 1);
            return cards.SingleOrDefault();
        }
        #endregion

        #region Attack
        public static Task Attack(this IActionHost host, Func<IActionHost, bool> filter, Action<IActionHost> act, bool benign = false)
        {
            return host.Attack(filter, target =>
            {
                act(target);
                return Task.CompletedTask;
            }, benign);
        }

        public static Task Attack(this IActionHost host, Action<IActionHost> act, bool benign = false)
        {
            return host.Attack(_ => true, act, benign);
        }

        public static Task Attack(this IActionHost host, Func<IActionHost, Task> act, bool benign = false)
        {
            return host.Attack(_ => true, act, benign);
        }
        #endregion
    }
}