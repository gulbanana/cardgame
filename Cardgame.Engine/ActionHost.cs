using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cardgame.All;
using Cardgame.API;
using Cardgame.Engine.Logging;
using Cardgame.Model;

namespace Cardgame.Engine
{
    internal abstract class ActionHost : IActionHost
    {   
        protected readonly GameEngine engine;     
        protected readonly IRecord logRecord;
        public string Player { get; }
        public string PreviousPlayer => engine.Model.PreviousPlayer;
        public int ActionCount => engine.ActionsThisTurn;
        public int ShuffleCount { get; private set; }

        public ActionHost(GameEngine engine, IRecord logRecord, string owningPlayer)
        {
            this.logRecord = logRecord;
            this.engine = engine;
            this.Player = owningPlayer;
        }

        protected abstract IActionHost CloneHost(IRecord logRecord, string owningPlayer);

        protected void LogVanilla(Action<Chunk> f)
        {
            f(logRecord.LatestChunk);
            logRecord.Update();
        }

        protected void LogMovement(Motion type, string[] cards, Zone from, Zone to)
        {            
            if (logRecord.LatestChunk.HasVanillaContent())
            {
                logRecord.CloseChunk();
            }

            logRecord.LatestChunk.Movements.Add(new Movement(type, cards, from, to));

            logRecord.Update();
        }

        private void LogLine(string eventText)
        {
            if (logRecord.LatestChunk.HasVanillaContent())
            {
                logRecord.CloseChunk();
            }

            logRecord.LatestChunk.Lines.Add(eventText);

            logRecord.Update();
        }

        protected string FormatInitialVerb(string secondPerson, string thirdPerson, string continuous)
        {
            if (Player == engine.Model.ActivePlayer)
            {
                return $"<if you='you {secondPerson}' them='{continuous}'>{Player}</if>";
            }
            else
            {
                return $"<player>{Player}</player><if you='{secondPerson}' them='{thirdPerson}'>{Player}</if>";
            }
        }
    
        // XXX should go in logmanager.. if reorders are even worth logging. they always seem to happen as a result of putting cards back.
        private string FormatSourceForReorder(Zone from)
        {
            switch (from.Name)
            {
                case ZoneName.DeckBottom:
                    return $@"<run>the bottom of</run>
                    <if you='your' them='their'>{Player}</if>
                    <run>deck.</run>";

                case ZoneName.Deck:
                case ZoneName.DeckTop:
                    return $@"<run>the top of</run>
                    <if you='your' them='their'>{Player}</if>
                    <run>deck.</run>";

                case ZoneName.Discard:
                    return $@"<if you='your' them='their'>{Player}</if>
                    <run>discard pile.</run>";

                case ZoneName.Hand:
                    return $@"<if you='your' them='their'>{Player}</if>
                    <run>hand.</run>";

                case ZoneName.InPlay:
                    return "<run>in play.</run>";

                case ZoneName.Revealed:
                    return "<run>the revealed cards.</run>";

                case ZoneName.Trash:
                    return $@"<run>the trash.</run>";

                default:
                    throw new CommandException($"Unknown source zone {from}");
            }
        }

        private void NoteReshuffle()
        {
            ShuffleCount++;
            LogVanilla(chunk => chunk.Reshuffled = true);
        }

        IModifier[] IActionHost.GetModifiers() 
        {
            return engine.Model.GetModifiers();
        }

        public IActionHost Isolate()
        {
            return CloneHost(logRecord.CreateSubrecord(Player, inline: false), Player);
        }
    
        ICard[] IActionHost.Examine(Zone @in, string player)
        {
            return engine.GetCards(player ?? Player, @in, NoteReshuffle).Select(All.Cards.ByName).ToArray();
        }

        int IActionHost.Count(Zone @in)
        {
            return engine.CountCards(Player, @in);
        }

        void IActionHost.AddActions(int n)
        {
            engine.Model.ActionsRemaining += n;
            LogVanilla(chunk => chunk.AddedActions += n);
        }

        void IActionHost.AddBuys(int n)
        {
            engine.Model.BuysRemaining += n;
            LogVanilla(chunk => chunk.AddedBuys += n);
        }

        void IActionHost.AddCoins(int n)
        {
            engine.Model.CoinsRemaining += n;
            LogVanilla(chunk => chunk.AddedCoins += n);
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

            LogVanilla(chunk => chunk.AddedCards.AddRange(drawn));

            return drawn.Select(All.Cards.ByName).ToArray();
        }

        void IActionHost.Discard(string[] cards, Zone from)
        {
            if (!cards.Any()) return;

            foreach (var card in cards)
            {
                engine.MoveCard(Player, card, from, Zone.Discard);
            }

            LogMovement(Motion.Discard, cards, from, Zone.Discard);
        }

        void IActionHost.Trash(string[] cards, Zone from)
        {
            if (!cards.Any()) return;

            foreach (var card in cards)
            {
                engine.MoveCard(Player, card, from, Zone.Trash);
            }

            LogMovement(Motion.Trash, cards, from, Zone.Trash);
        }

        async Task IActionHost.Gain(string[] cards, Zone to)
        {
            if (!cards.Any()) return;

            var actuallyGained = new List<string>();
            foreach (var card in cards)
            {
                if (engine.Model.Supply[card] > 0)
                {             
                    await engine.GainCardAsync(logRecord, Player, card, Zone.SupplyAvailable, to);
                    actuallyGained.Add(card);
                }
                else
                {
                    LogLine($@"
                        {FormatInitialVerb("don&apos;t gain", "doesn&apos;t gain", "not gaining")}
                        <card suffix=','>{card}</card>
                        <run>because there are none available.</run>
                    ");
                }
            }

            if (actuallyGained.Any())
            {
                LogMovement(Motion.Gain, actuallyGained.ToArray(), Zone.SupplyAvailable, to);
            }
        }

        async Task IActionHost.GainFrom(string[] cards, Zone from)
        {
            if (!cards.Any()) return;

            foreach (var id in cards)
            {
                await engine.GainCardAsync(logRecord, Player, id, from, Zone.Discard);
            }

            LogMovement(Motion.Gain, cards, from, Zone.Discard);
        }

        void IActionHost.PutOnDeck(string[] cards, Zone from)
        {
            if (!cards.Any()) return;

            foreach (var card in cards)
            {
                engine.MoveCard(Player, card, from, Zone.Deck);
            }

            LogMovement(Motion.Put, cards, from, Zone.Deck);
        }

        void IActionHost.PutIntoHand(string[] cards, Zone from)
        {
            if (!cards.Any()) return;

            foreach (var card in cards)
            {
                engine.MoveCard(Player, card, from, Zone.Hand);
            }

            LogMovement(Motion.Put, cards, from, Zone.Deck);
        }

        void IActionHost.PutOnMat(string mat, string[] cards, Zone from)
        {
            if (!cards.Any()) return;

            foreach (var card in cards)
            {
                engine.MoveCard(Player, card, from, Zone.PlayerMat(mat));
            }

            LogMovement(Motion.Put, cards, from, Zone.PlayerMat(mat));
        }

        void IActionHost.ReturnToSupply(string[] cards)
        {
            if (!cards.Any()) return;
            
            foreach (var card in cards)
            {
                engine.MoveCard(Player, card, Zone.Hand, Zone.SupplyAvailable);
            }

            LogMovement(Motion.Return, cards, Zone.Hand, Zone.SupplyAvailable);
        }

        // XXX should probably put things into the Revealed zone!
        void IActionHost.Reveal(string[] cards, Zone from)
        {
            if (!cards.Any()) return;
            
            LogMovement(Motion.Reveal, cards, from, Zone.Revealed);
        }

        ICard[] IActionHost.RevealUntil(Func<ICard, bool> predicate)
        {
            var found = false;
            var cards = new List<ICard>();

            while (!found && (engine.CountCards(Player, Zone.Deck) + engine.CountCards(Player, Zone.Discard) > 0))
            {
                var top = engine.GetCards(Player, Zone.DeckTop(1), NoteReshuffle).Single();
                engine.MoveCard(Player, top, Zone.Deck, Zone.Revealed);

                var card = All.Cards.ByName(top);
                cards.Add(card);
                if (predicate(card)) 
                {
                    found = true;
                }
            }

            LogMovement(Motion.Reveal, cards.Names(), Zone.Deck, Zone.Revealed);

            return cards.ToArray();
        }

        void IActionHost.Name(string card)
        {
            LogLine($@"
                {FormatInitialVerb("name", "names", "naming")}
                <card suffix='.'>{card}</card>
            ");
        }

        void IActionHost.Reorder(string[] cards, Zone @in)
        {
            if (!cards.Any()) return;

            var existingCards = engine.GetCards(Player, @in, NoteReshuffle);
            if (existingCards.Length != cards.Length || !existingCards.OrderBy(id => id).SequenceEqual(cards.OrderBy(id => id)))
            {
                throw new Exception("reordered cards are not the correct set");
            }

            engine.SetCardOrder(Player, cards, @in);

            LogLine($@"
                {FormatInitialVerb("reorder", "reorders", "reordering")}
                {FormatSourceForReorder(@in)}
            ");
        }

        async Task IActionHost.PlayCard(string card, Zone from)
        {
            LogMovement(Motion.Play, new[] { card }, from, Zone.InPlay);

            await engine.PlayCardAsync(logRecord.CreateSubrecord(Player, inline: false), Player, card, from);
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

        async Task<ICard[]> IActionHost.OrderCards(string prompt, string[] cards)
        {
            var ids = await engine.Choose<string[], string[]>(
                Player,
                ChoiceType.OrderCards,
                prompt,
                cards
            );

            return ids.Select(All.Cards.ByName).ToArray();
        }

        async Task IActionHost.AllPlayers(Func<IActionHost, bool> filter, Func<IActionHost, Task> act, bool isAttack)
        {
            var targetPlayers = engine.Model.Players
                .Select(player => CloneHost(logRecord.CreateSubrecord(player, inline: true), player))
                .Where(filter)
                .ToList();
            
            foreach (var target in targetPlayers)
            {
                if (isAttack)
                {
                    if (engine.Model.PreventedAttacks.Contains(target.Player))
                    {
                        engine.Model.PreventedAttacks.Remove(target.Player);
                    }
                    else
                    {
                        await act(target);
                    }
                }
                else
                {
                    await act(target);
                }
            }
        }

        void IActionHost.PreventNextAttack()
        {
            engine.Model.PreventedAttacks.Add(Player);
        }

        string IActionHost.GetPlayerToLeft()
        {
            var self = Array.FindIndex(engine.Model.Players, e => e == Player);
            var left = self + 1; // clockwise
            if (left >= engine.Model.Players.Length) left = 0;
            return engine.Model.Players[left];
        }

        string IActionHost.GetPlayerToRight()
        {
            var self = Array.FindIndex(engine.Model.Players, e => e == Player);
            var right = self - 1; // clockwise
            if (right < 0) right = engine.Model.Players.Length - 1;
            return engine.Model.Players[right];
        }

        void IActionHost.AddEffect(string effect)
        {
            engine.Model.ActiveEffects.Add(effect);
        }

        void IActionHost.RemoveEffect(string effect)
        {
            engine.Model.ActiveEffects.Remove(effect);
        }

        void IActionHost.AddToken(string effect, string pile)
        {
            engine.Model.SupplyTokens[pile] = engine.Model.SupplyTokens[pile].Append("EmbargoToken").ToArray();
            
            var token = All.Effects.ByName(effect);
            var description = (token as IToken)?.Description ?? token.Name;
            
            LogLine($@"
                {FormatInitialVerb("put", "puts", "putting")}
                <run>{description} on</run>
                <card suffix='.'>{pile}</card>.
            ");
        }
        
        public virtual void Attach(string card, Zone from)
        {
            throw new NotSupportedException("Current ActionHost is not a card.");
        }

        public virtual void Detach(Zone to)
        {
            throw new NotSupportedException("Current ActionHost is not a card.");
        }

        public virtual void CompleteDuration()
        {
            throw new NotSupportedException("Current ActionHost is not a Duration card.");
        }
     
        // this is a special case used by Chancellor: it does not count as 'discarding' each card, 
        // and you may not see what cards were discarded
        void IActionHost.DiscardEntireDeck()
        {
            var deck = engine.Model.Decks[Player];
            while (deck.Any())
            {
                engine.MoveCard(Player, deck[0], Zone.Deck, Zone.Discard);
            }

            LogLine($@"
                {FormatInitialVerb("put", "puts", "putting")}
                <if you='your' them='their'>{Player}</if>
                <run>deck into</run>
                <if you='your' them='their'>{Player}</if>
                <run>discard pile.</run>
            ");
        }

        // this is a special case used by Masquerade, but could be generalised
        void IActionHost.PassCard(string toPlayer, string card)
        {
            var stash = Zone.Stash();
            engine.MoveCard(Player, card, Zone.Hand, stash);
            engine.MoveCard(toPlayer, card, stash, Zone.Hand);

            LogLine($@"
                {FormatInitialVerb("pass", "passes", "passing")}
                <run>a card to</run>
                <player suffix='.'>{toPlayer}</player>
            ");
        }

        // this is a special case used by Secret Passage
        void IActionHost.InsertIntoDeck(string card, int position)
        {
            var instance = engine.Model.Hands[Player].Extract(card);
            engine.Model.Decks[Player].Insert(position, instance);
            
            if (position == 0)
            {
                LogLine($@"
                    {FormatInitialVerb("put", "puts", "putting")}
                    <run>a card onto</run>
                    <if you='your' them='their'>{Player}</if>
                    <run>deck.</run>
                ");
            }
            else if (position == engine.Model.Decks[Player].Count - 1)
            {
                LogLine($@"
                    {FormatInitialVerb("put", "puts", "putting")}
                    <run>a card on the bottom of</run>
                    <if you='your' them='their'>{Player}</if>
                    <run>deck.</run>                
                ");
            }
            else
            {
                LogLine($@"
                    {FormatInitialVerb("put", "puts", "putting")}
                    <run>a card into</run>
                    <if you='your' them='their'>{Player}</if>
                    <run>deck, {position} cards down.</run>
                ");
            }
        }

        // used by Outpost and Possession
        public void InsertExtraTurn(string player)
        {
            engine.ExtraTurns.Enqueue(player);
        }
    }
}