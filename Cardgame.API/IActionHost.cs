using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cardgame.API
{
    public interface IActionHost
    {
        int IndentLevel { get; set; }
        string Player { get; }
        string PreviousPlayer { get; }
        bool IsActive { get; }
        int ShuffleCount { get; }
        int ActionCount { get; }
        IModifier[] GetModifiers();

        // the four vanilla operations
        ICard[] DrawCards(int n);
        void AddActions(int n);
        void AddBuys(int n);
        void AddCoins(int n);
        
        // move cards around
        void Discard(string[] cards, Zone from);
        void Trash(string[] cards, Zone from);
        void Gain(string[] cards, Zone to);        
        void GainFrom(string[] cards, Zone from);
        void PutOnDeck(string[] cards, Zone from);
        void PutIntoHand(string[] cards, Zone from);
        void PutOnMat(string mat, string[] cards, Zone from);
        void ReturnToSupply(string[] cards);
        void Reveal(string[] cards, Zone from);
        ICard[] RevealUntil(Func<ICard, bool> predicate);
        void Name(string card);

        // manipulate entire zones
        ICard[] Examine(Zone @in, string player = null);
        int Count(Zone @in);
        void Reorder(string[] cards, Zone @in);

        // interaction
        Task PlayCard(string card, Zone from);
        Task AllPlayers(Func<IActionHost, bool> filter, Func<IActionHost, Task> act, bool isAttack = false);
        void PreventAttack(bool enable);
        string GetPlayerToLeft();
        string GetPlayerToRight();

        // user inputs
        Task<bool> YesNo(string prompt, string message);
        Task ChooseOne(string prompt, params NamedOption[] options);
        Task ChooseMultiple(string prompt, int number, params NamedOption[] options);
        Task<T[]> SelectCards<T>(string prompt, Zone source, Func<IEnumerable<ICard>, IEnumerable<T>> filter, int? min, int? max) where T : ICard;
        Task<ICard[]> OrderCards(string prompt, string[] cards);

        // advanced
        void AddEffect(string effect);
        void RemoveEffect(string effect);
        void AddToken(string effect, string pile);
        void Attach(string card, Zone from);
        void Detach(Zone to);
        void CompleteDuration();

        // special cases
        void DiscardEntireDeck();
        void PassCard(string player, string card);
        void InsertIntoDeck(string card, int position);
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
            host.Trash(cards.Names(), from);
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
            host.Trash(cards.Names(), Zone.Hand);
        }

        public static void Trash(this IActionHost host, ICard card)
        {
            host.Trash(new[] { card.Name }, Zone.Hand);
        }

        public static ICard[] Trash(this IActionHost host, Zone from)
        {
            var cards = host.Examine(from);
            if (cards.Any())
            {
                host.Trash(cards, from);
            }
            return cards;
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
            host.Discard(cards.Names(), from);
        }

        public static void Discard(this IActionHost host, ICard card, Zone from)
        {
            host.Discard(card.Name, from);
        }

        public static void Discard(this IActionHost host, ICard[] cards)
        {
            host.Discard(cards.Names(), Zone.Hand);
        }

        public static void Discard(this IActionHost host, ICard card)
        {
            host.Discard(new[]{ card.Name }, Zone.Hand);
        }

        public static ICard[] Discard(this IActionHost host, Zone from)
        {
            var cards = host.Examine(from);
            if (cards.Any())
            {
                host.Discard(cards, from);
            }
            return cards;
        }
        #endregion Discard

        #region Gain
        public static void Gain(this IActionHost host, string card, Zone to)
        {
            host.Gain(new[] { card }, Zone.Discard);
        }

        public static void Gain(this IActionHost host, string card)
        {
            host.Gain(new[] { card }, Zone.Discard);
        }

        public static void Gain(this IActionHost host, ICard card, Zone to)
        {
            host.Gain(new[] { card.Name }, to);
        }

        public static void Gain(this IActionHost host, ICard card)
        {
            host.Gain(new[] { card.Name }, Zone.Discard);
        }

        public static void Gain(this IActionHost host, ICard[] cards)
        {
            host.Gain(cards.Names(), Zone.Discard);
        }

        public static void GainFrom(this IActionHost host, ICard[] cards, Zone from)
        {
            host.GainFrom(cards.Names(), from);
        }

        public static void GainFrom(this IActionHost host, ICard card, Zone from)
        {
            host.GainFrom(new[] { card.Name }, from);
        }
        #endregion

        #region PutOnDeck
        public static void PutOnDeck(this IActionHost host, string card, Zone from)
        {
            host.PutOnDeck(new[] { card }, from);
        }

        public static void PutOnDeck(this IActionHost host, ICard card, Zone from)
        {
            host.PutOnDeck(new[] { card.Name }, from);
        }

        public static void PutOnDeck(this IActionHost host, ICard[] cards, Zone from)
        {
            host.PutOnDeck(cards.Names(), from);
        }

        public static void PutOnDeck(this IActionHost host, ICard[] cards)
        {
            host.PutOnDeck(cards.Names(), Zone.Hand);
        }

        public static void PutOnDeck(this IActionHost host, ICard card)
        {
            host.PutOnDeck(new[] { card.Name }, Zone.Hand);
        }
        #endregion

        #region PutIntoHand
        public static void PutIntoHand(this IActionHost host, string card, Zone from)
        {
            host.PutIntoHand(new[] { card }, from);
        }

        public static void PutIntoHand(this IActionHost host, ICard[] cards, Zone from)
        {
            host.PutIntoHand(cards.Names(), from);
        }

        public static void PutIntoHand(this IActionHost host, ICard card, Zone from)
        {
            host.PutIntoHand(new[] { card.Name }, from);
        }

        public static void PutIntoHand(this IActionHost host, Zone from)
        {
            var cards = host.Examine(from);
            host.PutIntoHand(cards, from);
        }
        #endregion

        #region PutOnMat
        public static void PutOnMat(this IActionHost host, string mat, string card, Zone from)
        {
            host.PutOnMat(mat, new[] { card }, from);
        }

        public static void PutOnMat(this IActionHost host, string mat, ICard[] cards, Zone from)
        {
            host.PutOnMat(mat, cards.Names(), from);
        }

        public static void PutOnMat(this IActionHost host, string mat, ICard card, Zone from)
        {
            host.PutOnMat(mat, new[] { card.Name }, from);
        }
        #endregion

        #region ReturnToSupply
        public static void ReturnToSupply(this IActionHost host, ICard[] cards)
        {
            host.ReturnToSupply(cards.Names());
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
            host.Reveal(cards.Names(), from);
        }

        public static void Reveal(this IActionHost host, ICard[] cards)
        {
            host.Reveal(cards.Names(), Zone.Hand);
        }

        public static void Reveal(this IActionHost host, ICard card, Zone from)
        {
            host.Reveal(new[] { card.Name }, from);
        }

        public static void Reveal(this IActionHost host, ICard card)
        {
            host.Reveal(new[] { card.Name }, Zone.Hand);
        }

        public static ICard[] Reveal(this IActionHost host, Zone from)
        {
            var cards = host.Examine(from);
            if (cards.Any())
            {
                host.Reveal(cards, from);
            }
            return cards;
        }
        #endregion

        #region Name
        public static void Name(this IActionHost host, ICard card)
        {
            host.Name(card.Name);
        }
        #endregion

        public static void Reorder(this IActionHost host, ICard[] cards, Zone @in)
        {
            host.Reorder(cards.Names(), @in);
        }

        public static Task PlayCard(this IActionHost host, string card)
        {
            return host.PlayCard(card, Zone.Hand);
        }

        public static Task PlayCard(this IActionHost host, IActionCard card, Zone from)
        {
            return host.PlayCard(card.Name, from);
        }

        public static Task PlayCard(this IActionHost host, IActionCard card)
        {
            return host.PlayCard(card.Name, Zone.Hand);
        }

        public static Task ChooseOne(this IActionHost host, string prompt, IEnumerable<NamedOption> options)
        {
            return host.ChooseOne(prompt, options.ToArray());
        }

        #region SelectCards
        // simple predicate 
        public static Task<ICard[]> SelectCards(this IActionHost host, string prompt, Zone source, Func<ICard, bool> filter, int? min, int? max)
        {
            return host.SelectCards<ICard>(prompt, source, cs => cs.Where(filter), min, max);
        }

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

        public static async Task<ICard> SelectCard(this IActionHost host, string prompt, Zone source, Func<ICard, bool> filter)
        {
            var cards = await host.SelectCards<ICard>(prompt, source, cs => cs.Where(filter), 1, 1);
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

        #region OrderCards
        public static Task<ICard[]> OrderCards(this IActionHost host, string prompt, ICard[] cards)
        {
            return host.OrderCards(prompt, cards.Names());
        }

        public static async Task<ICard[]> OrderCards(this IActionHost host, string prompt, Zone @in)
        {
            var sourceCards = host.Examine(@in);
            return await host.OrderCards(prompt, sourceCards.Names());
        }
        #endregion

        #region AllPlayers
        public static Task AllPlayers(this IActionHost host, Func<IActionHost, Task> act, bool isAttack = false)
        {
            return host.AllPlayers(target => true, act, isAttack);
        }

        public static Task OtherPlayers(this IActionHost host, Func<IActionHost, bool> filter, Func<IActionHost, Task> act, bool isAttack = false)
        {
            return host.AllPlayers(target => filter(target) && target.Player != host.Player, act, isAttack);
        }

        public static Task OtherPlayers(this IActionHost host, Func<IActionHost, Task> act, bool isAttack = false)
        {
            return host.AllPlayers(target => target.Player != host.Player, act, isAttack);
        }

        public static Task OtherPlayers(this IActionHost host, Action<IActionHost> act, bool isAttack = false)
        {
            return host.AllPlayers(target => target.Player != host.Player, host =>
            {
                act(host);
                return Task.CompletedTask;
            }, isAttack);
        }

        public static Task OnePlayer(this IActionHost host, string player, Func<IActionHost, Task> act, bool isAttack = false)
        {
            return host.AllPlayers(target => target.Player == player, act, isAttack);
        }

        public static Task OnePlayer(this IActionHost host, string player, Action<IActionHost> act, bool isAttack = false)
        {
            return host.AllPlayers(target => target.Player == player, target =>
            {
                act(target);
                return Task.CompletedTask;
            }, isAttack);
        }

        public static Task Attack(this IActionHost host, Func<IActionHost, bool> filter, Func<IActionHost, Task> act)
        {
            return host.AllPlayers(target => filter(target) && target.Player != host.Player, act, true);
        }

        public static Task Attack(this IActionHost host, Func<IActionHost, bool> filter, Action<IActionHost> act)
        {
            return host.AllPlayers(target => filter(target) && target.Player != host.Player, target =>
            {
                act(target);
                return Task.CompletedTask;
            }, true);
        }

        public static Task Attack(this IActionHost host, Func<IActionHost, Task> act)
        {
            return host.AllPlayers(target => target.Player != host.Player, act, true);
        }

        public static Task Attack(this IActionHost host, Action<IActionHost> act)
        {
            return host.AllPlayers(target => target.Player != host.Player, target =>
            {
                act(target);
                return Task.CompletedTask;
            }, true);
        }
        #endregion

        public static void Attach(this IActionHost host, ICard card, Zone from)
        {
            host.Attach(card.Name, from);
        }
    }
}