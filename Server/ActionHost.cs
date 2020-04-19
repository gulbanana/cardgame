using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;
using Cardgame.Choices;
using Cardgame.Shared;

namespace Cardgame.Server
{
    internal class ActionHost : IActionHost
    {        
        private readonly int level;
        private readonly GameEngine engine;
        public string Player { get; }
        public bool IsActive => engine.Model.ActivePlayer == Player;
        public int ShuffleCount { get; private set; }
        public int ActionCount => engine.ActionsThisTurn;

        public ActionHost(int level, GameEngine engine, string player)
        {
            this.level = level;
            this.engine = engine;
            this.Player = player;
        }

        private string LogVerbInitial(string secondPerson, string thirdPerson, string continuous)
        {
            return engine.LogVerbInitial(Player, secondPerson, thirdPerson, continuous);
        }

        private string LogVerb(string secondPerson, string thirdPerson, string continuous)
        {
            if (Player == engine.Model.ActivePlayer)
            {
                return $"<if you='{secondPerson}' them='{continuous}'>{Player}</if>";
            }
            else
            {
                return $"<if you='{secondPerson}' them='{thirdPerson}'>{Player}</if>";
            }
        }

        private string LogCardList(string[] ids, bool terminal = true)
        {
            if (!ids.Any())
            {
                return terminal ? "<run>nothing.</run>" : "<run>nothing</run>";
            }

            return string.Join(Environment.NewLine, ids.Select((id, ix) => 
            {
                var suffix = ix == ids.Length -1 ? (terminal ? "." : "")
                    : ix < ids.Length - 2 ? ","
                    : " and";
                return $"<card suffix='{suffix}'>{id}</card>";
            }));
        }

        private string LogSource(Zone from)
        {
            switch (from)
            {
                case Zone.DeckTop1:
                case Zone.DeckTop2:
                case Zone.DeckTop3:
                case Zone.DeckTop4:
                    return $@"<run>the top of</run>
                    <if you='your' them='their'>{Player}</if>
                    <run>deck.</run>";

                case Zone.Hand:
                    return $@"<if you='your' them='their'>{Player}</if>
                    <run>hand.</run>";

                case Zone.Discard:
                    return $@"<if you='your' them='their'>{Player}</if>
                    <run>discard pile.</run>";

                case Zone.Trash:
                    return $@"<run>the trash.</run>";

                default:
                    throw new CommandException($"Unknown source zone {from}");
            }
        }

        private string LogDestination(Zone to)
        {
            switch (to)
            {
                case Zone.DeckTop1:
                case Zone.DeckTop2:
                case Zone.DeckTop3:
                case Zone.DeckTop4:
                    return $@"{LogVerb("put", "puts", "putting")}
                              <run>it onto</run>
                              <if you='your' them='their'>{Player}</if>
                              <run>deck.</run>";

                case Zone.Hand:
                    return $@"{LogVerb("put", "puts", "putting")}
                              <run>it into</run>
                              <if you='your' them='their'>{Player}</if>
                              <run>hand.</run>";

                case Zone.Discard:
                    return $@"{LogVerb("discard", "discards", "discarding")}
                              <run>it.</run>";

                case Zone.Trash:
                    return $@"{LogVerb("trash", "trashes", "trashing")}
                              <run>it.</run>";

                default:
                    throw new CommandException($"Unknown destination zone {to}");
            }
        }

        private void NoteReshuffle()
        {
            ShuffleCount++;
            engine.LogPartialEvent($@"<spans>
                <indent level='{level}' />
                <if you='(you reshuffle.)' them='(reshuffling.)'>{Player}</if>
            </spans>");
        }

        ICard[] IActionHost.Examine(Zone @in)
        {
            return engine.GetCards(Player, @in, NoteReshuffle).Select(All.Cards.ByName).ToArray();
        }

        int IActionHost.Count(Zone @in)
        {
            return engine.CountCards(Player, @in);
        }

        void IActionHost.AddActions(int n)
        {
            engine.Model.ActionsRemaining += n;

            engine.LogPartialEvent($@"<spans>
                <indent level='{level}' />
                <if you='you get' them='getting'>{engine.Model.ActivePlayer}</if>
                <run>+{n} actions.</run>
            </spans>");
        }

        void IActionHost.AddBuys(int n)
        {
            engine.Model.BuysRemaining += n;

            engine.LogPartialEvent($@"<spans>
                <indent level='{level}' />
                <if you='you get' them='getting'>{engine.Model.ActivePlayer}</if>
                <run>+{n} buys.</run>
            </spans>");
        }

        void IActionHost.AddMoney(int n)
        {
            engine.Model.MoneyRemaining += n;

            engine.LogPartialEvent($@"<spans>
                <indent level='{level}' />
                <if you='you get' them='getting'>{engine.Model.ActivePlayer}</if>
                <run>+${n}.</run>
            </spans>");
        }

        ICard[] IActionHost.DrawCards(int n)
        {
            var drawn = new List<string>();

            var reshuffled = false;
            for (var i = 0; i < n; i++)
            {
                reshuffled = reshuffled | engine.ReshuffleIfEmpty(Player);
                var id = engine.DrawCardIfAny(Player);
                if (id != null)
                {
                    drawn.Add(id);
                }
                else
                {
                    break;
                }
            }
            if (reshuffled)
            {
                NoteReshuffle();
            }

            engine.LogPartialEvent($@"<spans>
                <indent level='{level}' />
                {LogVerbInitial("draw", "draws", "drawing")}
                <run>{(n == 1 ? "a card." : $"{n} cards.")}</run>
            </spans>");

            return drawn.Select(All.Cards.ByName).ToArray();
        }

        void IActionHost.Discard(string[] cards, Zone from)
        {
            foreach (var card in cards)
            {
                engine.MoveCard(Player, card, from, Zone.Discard);
            }

            if (from == Zone.Hand)
            {
                engine.LogPartialEvent($@"<spans>
                    <indent level='{level}' />
                    {LogVerbInitial("discard", "discards", "discarding")}
                    {LogCardList(cards)}
                </spans>");
            }
            else
            {
                engine.LogPartialEvent($@"<spans>
                <indent level='{level}' />
                    {LogVerbInitial("discard", "discards", "discarding")}
                    {LogCardList(cards, terminal: false)}
                    <run>from</run>
                    {LogSource(from)}
                </spans>");            
            }
        }

        void IActionHost.Trash(string[] cards, Zone from)
        {
            foreach (var card in cards)
            {
                engine.MoveCard(Player, card, from, Zone.Trash);
            }

            engine.LogPartialEvent($@"<spans>
                <indent level='{level}' />
                {LogVerbInitial("trash", "trashes", "trashing")}
                {LogCardList(cards)}
            </spans>");        
        }

        void IActionHost.Gain(string id, Zone to)
        {
            if (engine.Model.Supply[id] == 0)
            {
                engine.LogPartialEvent($@"<spans>
                    <indent level='{level}' />
                    {LogVerbInitial("don&apos;t gain", "doesn&apos;t gain", "not gaining")}
                    <card suffix=','>{id}</card>
                    <run>because there are none available.</run>
                </spans>");
                return;
            }

            engine.MoveCard(Player, id, Zone.SupplyAvailable, to);

            if (to == Zone.Discard)
            {
                engine.LogPartialEvent($@"<spans>
                    <indent level='{level}' />
                    {LogVerbInitial("gain", "gains", "gaining")}
                    <card suffix='.'>{id}</card>
                </spans>");
            }
            else
            {
                engine.LogPartialEvent($@"<spans>
                    <indent level='{level}' />
                    {LogVerbInitial("gain", "gains", "gaining")}
                    <card>{id}</card>
                    <run>and</run>
                    {LogDestination(to)}
                </spans>");
            }
        }

        void IActionHost.GainFrom(string[] cards, Zone from)
        {
            foreach (var id in cards)
            {
                engine.MoveCard(Player, id, from, Zone.Discard);
            }
            
            engine.LogPartialEvent($@"<spans>
                <indent level='{level}' />
                {LogVerbInitial("gain", "gains", "gaining")}
                {LogCardList(cards, terminal: false)}
                <run>from</run>
                {LogSource(from)}
            </spans>");
        }

        void IActionHost.PlaceOnDeck(string card, Zone from)
        {
            engine.MoveCard(Player, card, from, Zone.DeckTop1);
            
            if (from == Zone.Hand)
            {
                engine.LogPartialEvent($@"<spans>
                    <indent level='{level}' />
                    {LogVerbInitial("put", "puts", "putting")}
                    <card>{card}</card>
                    <run>onto</run>
                    <if you='your' them='their'>{Player}</if>
                    <run>deck.</run>
                </spans>");
            }
            else
            {
                engine.LogPartialEvent($@"<spans>
                    <indent level='{level}' />
                    {LogVerbInitial("put", "puts", "putting")}
                    <card>{card}</card>
                    <run>onto</run>
                    <if you='your' them='their'>{Player}</if>
                    <run>deck from</run>
                    {LogSource(from)}
                </spans>");
            }
        }

        void IActionHost.PutIntoHand(string[] cards, Zone from)
        {
            foreach (var card in cards)
            {
                engine.MoveCard(Player, card, from, Zone.Hand);
            }
            
            engine.LogPartialEvent($@"<spans>
                <indent level='{level}' />
                {LogVerbInitial("put", "puts", "putting")}
                {LogCardList(cards, terminal: false)}
                <run>into</run>
                <if you='your' them='their'>{Player}</if>
                <run>hand.</run>
            </spans>");
        }

        void IActionHost.Reveal(string[] cards, Zone from)
        {
            engine.LogPartialEvent($@"<spans>
                <indent level='{level}' />
                {LogVerbInitial("reveal", "reveals", "revealing")}
                {LogCardList(cards, terminal: false)}
                <run>from</run>
                {LogSource(from)}
            </spans>");
        }

        void IActionHost.Name(string card)
        {
            engine.LogPartialEvent($@"<spans>
                <indent level='{level}' />
                {LogVerbInitial("name", "names", "naming")}
                <card suffix='.'>{card}</card>
            </spans>");
        }

        void IActionHost.Reorder(string[] cards, Zone @in)
        {
            var existingCards = engine.GetCards(Player, @in, NoteReshuffle);
            if (existingCards.Length != cards.Length || !existingCards.OrderBy(id => id).SequenceEqual(cards.OrderBy(id => id)))
            {
                throw new Exception("reordered cards are not the correct set");
            }

            engine.SetCards(Player, cards, @in);

            engine.LogPartialEvent($@"<spans>
                <indent level='{level}' />
                {LogVerbInitial("reorder", "reorders", "reordering")}
                {LogSource(@in)}
            </spans>");
        }

        async Task IActionHost.PlayCard(string card, Zone from)
        {
            engine.LogPartialEvent($@"<spans>
                <indent level='{level}' />
                {LogVerbInitial("play", "plays", "playing")}
                <card suffix='.'>{card}</card>
            </spans>");

            await engine.PlayCardAsync(level + 1, Player, card, from);
        }

        Task<bool> IActionHost.YesNo(string prompt, string message)
        {
            return engine.Choose<string, bool>(
                Player,
                ChoiceType.YesNo,
                prompt,
                message
            );
        }

        public async Task ChooseOne(string prompt, params NamedOption[] options)
        {
            var result = await engine.Choose<string[], int>(
                Player,
                ChoiceType.ChooseOne,
                prompt,
                options.Select(o => o.Text).ToArray()
            );

            await options[result].Execute();
        }

        public async Task ChooseMultiple(string prompt, int number, params NamedOption[] options)
        {
            var results = await engine.Choose<ChooseMultipleInput, int[]>(
                Player,
                ChoiceType.ChooseMultiple,
                prompt,
                new ChooseMultipleInput { Number = number, Choices = options.Select(o => o.Text).ToArray() }
            );

            foreach (var result in results)
            {
                await options[result].Execute();
            }
        }

        async Task<T[]> IActionHost.SelectCards<T>(string prompt, Zone source, Func<IEnumerable<ICard>, IEnumerable<T>> filter, int? min, int? max)
        {            
            var sourceCards = engine.GetCards(Player, source, NoteReshuffle);

            var filteredCards = filter(sourceCards.Select(All.Cards.ByName));
            if (!filteredCards.Any())
            {
                return Array.Empty<T>();
            }

            var ids = await engine.Choose<SelectCardsInput, string[]>(
                Player,
                ChoiceType.SelectCards, 
                prompt,
                new SelectCardsInput
                {
                    Choices = filteredCards.Select(card => card.Name).ToArray(),
                    Min = min,
                    Max = max
                }
            );

            return ids.Select(All.Cards.ByName).Cast<T>().ToArray();
        }

        async Task<ICard[]> IActionHost.OrderCards(string prompt, Zone source)
        {
            var sourceCards = engine.GetCards(Player, source, NoteReshuffle);

            var ids = await engine.Choose<string[], string[]>(
                Player,
                ChoiceType.OrderCards,
                prompt,
                sourceCards
            );

            return ids.Select(All.Cards.ByName).ToArray();
        }

        async Task IActionHost.AllPlayers(Func<IActionHost, bool> filter, Func<IActionHost, Task> act, bool isAttack)
        {
            var targetPlayers = engine.Model.Players
                .Select(player => new ActionHost(1, engine, player))
                .Where(filter)
                .ToList();
            
            foreach (var target in targetPlayers)
            {
                if (!isAttack || !engine.Model.PreventedAttacks.Contains(target.Player))
                {
                    await act(target);
                }
            }
        }

        string IActionHost.GetPlayerToLeft()
        {
            var self = Array.FindIndex(engine.Model.Players, e => e == Player);
            var left = self + 1; // clockwise
            if (left >= engine.Model.Players.Length) left = 0;
            return engine.Model.Players[left];
        }

        public IModifier[] GetModifiers() 
        {
            return engine.Model.GetModifiers();
        }
    
        void IActionHost.AddEffect(string effect)
        {
            engine.Model.ActiveEffects.Add(effect);
        }

        void IActionHost.RemoveEffect(string effect)
        {
            engine.Model.ActiveEffects.Remove(effect);
        }
     
        // this is a special case used by Chancellor: it does not count as 'discarding' each card, 
        // and you may not see what cards were discarded
        void IActionHost.DiscardEntireDeck()
        {
            var deck = engine.Model.Decks[Player];
            while (deck.Any())
            {
                engine.MoveCard(Player, deck[0], Zone.DeckTop1, Zone.Discard);
            }

            engine.LogPartialEvent($@"<spans>
                <indent level='{level}' />
                {LogVerbInitial("put", "puts", "putting")}
                <if you='your' them='their'>{Player}</if>
                <run>deck into</run>
                <if you='your' them='their'>{Player}</if>
                <run>discard pile.</run>
            </spans>");
        }

        // this is a special case used by Moat: it triggers *before* choices about an attack are made,
        // but only for the duration of that one card's attack
        void IActionHost.PreventAttack(bool enable)
        {
            if (enable)
            {
                engine.Model.PreventedAttacks.Add(Player);
            }
            else
            {
                engine.Model.PreventedAttacks.Remove(Player);
            }
        }

        // this is a special case used by Masquerade, but could be generalised
        void IActionHost.PassCard(string player, string card)
        {
            engine.Model.Hands[Player].Remove(card);
            engine.Model.Hands[player].Add(card);

            engine.LogPartialEvent($@"<spans>
                <indent level='{level}' />
                {LogVerbInitial("pass", "passes", "passing")}
                <run>a card to</run>
                <player suffix='.'>{player}</player>
            </spans>");
        }

        // this is a special case used by Secret Passage
        void IActionHost.InsertIntoDeck(string card, int position)
        {
            engine.Model.Hands[Player].Remove(card);
            engine.Model.Decks[Player].Insert(position, card);
            
            if (position == 0)
            {
                engine.LogPartialEvent($@"<spans>
                    <indent level='{level}' />
                    {LogVerbInitial("put", "puts", "putting")}
                    <run>a card onto</run>
                    <if you='your' them='their'>{Player}</if>
                    <run>deck.</run>
                </spans>");
            }
            else if (position == engine.Model.Decks[Player].Count - 1)
            {
                engine.LogPartialEvent($@"<spans>
                    <indent level='{level}' />
                    {LogVerbInitial("put", "puts", "putting")}
                    <run>a card on the bottom of</run>
                    <if you='your' them='their'>{Player}</if>
                    <run>deck.</run>                
                </spans>");
            }
            else
            {
                engine.LogPartialEvent($@"<spans>
                    <indent level='{level}' />
                    {LogVerbInitial("put", "puts", "putting")}
                    <run>a card into</run>
                    <if you='your' them='their'>{Player}</if>
                    <run>deck, {position} cards down.</run>
                </spans>");
            }
        }
    }
}