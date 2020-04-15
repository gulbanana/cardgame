using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cardgame
{
    internal class ActionHost : IActionHost
    {
        private readonly GameEngine engine;
        public string Player { get; }
        public int ShuffleCount { get; private set; }

        public ActionHost(GameEngine engine, string player)
        {
            this.engine = engine;
            this.Player = player;
        }

        private string LogVerbInitial(string secondPerson, string thirdPerson, string continuous)
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

        private string LogCardList(string[] ids)
        {
            return string.Join(Environment.NewLine, ids.Select((id, ix) => 
            {
                var suffix = ix == ids.Length -1 ? "."
                    : ix < ids.Length - 2 ? ","
                    : " and";
                return $"<card suffix='{suffix}'>{id}</card>";
            }));
        }

        private string LogDestination(Zone to)
        {
            switch (to)
            {
                case Zone.TopDeck:
                    return $@"{LogVerb("put", "puts", "putting")}
                              <run>it on top of</run>
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
                    throw new CommandException($"Unknown zone {to}");
            }
        }

        private void NoteReshuffle()
        {
            ShuffleCount++;
            engine.LogPartialEvent($@"<spans>
                <run>...</run>
                <if you='(you reshuffle.)' them='(reshuffling.)'>{Player}</if>
            </spans>");
        }

        Cards.CardModel[] IActionHost.GetHand()
        {
            return engine.Model.Hands[Player].Select(Cards.All.ByName).ToArray();
        }

        void IActionHost.AddActions(int n)
        {
            engine.Model.ActionsRemaining += n;

            engine.LogPartialEvent($@"<spans>
                <run>...</run>
                <if you='you get' them='getting'>{engine.Model.ActivePlayer}</if>
                <run>+{n} actions.</run>
            </spans>");
        }

        void IActionHost.AddBuys(int n)
        {
            engine.Model.BuysRemaining += n;

            engine.LogPartialEvent($@"<spans>
                <run>...</run>
                <if you='you get' them='getting'>{engine.Model.ActivePlayer}</if>
                <run>+{n} buys.</run>
            </spans>");
        }

        void IActionHost.AddMoney(int n)
        {
            engine.Model.MoneyRemaining += n;

            engine.LogPartialEvent($@"<spans>
                <run>...</run>
                <if you='you get' them='getting'>{engine.Model.ActivePlayer}</if>
                <run>+${n}.</run>
            </spans>");
        }

        void IActionHost.DrawCards(int n)
        {
            var reshuffled = false;
            for (var i = 0; i < n; i++)
            {
                reshuffled = reshuffled | engine.DrawCard(Player);
            }
            if (reshuffled)
            {
                NoteReshuffle();
            }

            engine.LogPartialEvent($@"<spans>
                <run>...</run>
                {LogVerbInitial("draw", "draws", "drawing")}
                <run>{(n == 1 ? "a card." : $"{n} cards.")}</run>
            </spans>");
        }

        void IActionHost.Discard(string[] cards, Zone from)
        {
            foreach (var card in cards)
            {
                engine.MoveCard(Player, card, from, Zone.Discard);
            }

            engine.LogPartialEvent($@"<spans>
                <run>...</run>
                {LogVerbInitial("discard", "discards", "discarding")}
                {LogCardList(cards)}
            </spans>");
        }

        void IActionHost.Trash(string[] cards)
        {
            foreach (var card in cards)
            {
                engine.MoveCard(Player, card, Zone.Hand, Zone.Trash);
            }

            engine.LogPartialEvent($@"<spans>
                <run>...</run>
                {LogVerbInitial("trash", "trashes", "trashing")}
                {LogCardList(cards)}
            </spans>");        
        }

        void IActionHost.Gain(string id, Zone to)
        {
            engine.MoveCard(Player, id, Zone.Kingdom, to);

            if (to == Zone.Discard)
            {
                engine.LogPartialEvent($@"<spans>
                    <run>...</run>
                    {LogVerbInitial("gain", "gains", "gaining")}
                    <card suffix='.'>{id}</card>
                </spans>");
            }
            else
            {
                engine.LogPartialEvent($@"<spans>
                    <run>...</run>
                    {LogVerbInitial("gain", "gains", "gaining")}
                    <card>{id}</card>
                    <run>and</run>
                    {LogDestination(to)}
                </spans>");
            }
        }

        void IActionHost.Draw(string card)
        {
            engine.MoveCard(Player, card, Zone.TopDeck, Zone.Hand);

            engine.LogPartialEvent($@"<spans>
                <run>...</run>
                {LogVerbInitial("draw", "draws", "drawing")}
                <card suffix='.'>{card}</card>
            </spans>");        
        }

        Cards.CardModel[] IActionHost.RevealAll(Zone from)
        {
            if (from == Zone.TopDeck)
            {
                var reshuffled = engine.DrawCard(Player, to: Zone.TopDeck);
                if (reshuffled)
                {
                    NoteReshuffle();
                }
            }

            var revealed = from switch {
                Zone.TopDeck => engine.Model.Decks[Player].Take(1),
                Zone.Hand => engine.Model.Hands[Player],
                Zone other => throw new CommandException($"Unknown zone {other}")
            };

            engine.LogPartialEvent($@"<spans>
                <run>...</run>
                {LogVerbInitial("reveal", "reveals", "revealing")}
                {LogCardList(revealed.ToArray())}
            </spans>");

            return revealed.Select(Cards.All.ByName).ToArray();
        }
        
        void IActionHost.RevealAndMove(string card, Zone from, Zone to)
        {
            engine.MoveCard(Player, card, from, to);

            engine.LogPartialEvent($@"<spans>
                <run>...</run>
                {LogVerbInitial("reveal", "reveals", "revealing")}
                <card>{card}</card>
                <run>and</run>
                {LogDestination(to)}
            </spans>");   
        }

        async Task<T> IActionHost.SelectCard<T>(string prompt, Zone source, Func<IEnumerable<Cards.CardModel>, IEnumerable<T>> filter)
        {
            var sourceCards = source switch 
            {
                Zone.Hand => engine.Model.Hands[Player],
                Zone.Kingdom => engine.Model.KingdomCards.Concat(new[]{"Estate", "Duchy", "Province", "Copper", "Silver", "Gold"}).Where(id => engine.Model.Stacks[id] > 0),
                Zone other => throw new CommandException($"Unknown CardSource {other}")
            };

            var filteredCards = filter(sourceCards.Select(Cards.All.ByName));

            var id = await engine.Choose<string[], string>(
                Player,
                ChoiceType.SelectCard, 
                $"<run>{prompt}</run>", 
                filteredCards.Select(card => card.Name).ToArray()
            );

            return filteredCards.First(card => card.Name == id);
        }

        Task<string[]> IActionHost.SelectCardsFromHand(string prompt, int? number)
        {
            return engine.Choose<SelectCardsInput, string[]>(
                Player,
                ChoiceType.SelectCards, 
                $"<run>{prompt}</run>",
                new SelectCardsInput
                {
                    Choices = engine.Model.Hands[Player].ToArray(),
                    NumberRequired = number
                }
            );
        }

        Task<bool> IActionHost.YesNo(string prompt)
        {
            return engine.Choose<bool, bool>(
                Player,
                ChoiceType.YesNo,
                $"<run>{prompt}</run>",
                false
            );
        }

        async Task IActionHost.Attack(Func<IActionHost, bool> filter, Func<IActionHost, Task> act)
        {
            var targetPlayers = engine.Model.Players
                .Except(new[]{ Player })
                .Select(player => new ActionHost(engine, player))
                .Where(filter)
                .ToList();
            
            foreach (var target in targetPlayers)
            {
                var hand = target.GetHand().ToList();
                var reactions = hand
                    .OfType<Cards.ActionCardModel>()
                    .Where(card => card.ReactionTrigger == TriggerType.Attack);

                var replaced = false;
                while (reactions.Any())
                {
                    var potentialReaction = reactions.First();
                    hand.RemoveAt(hand.FindIndex(e => e.Equals(potentialReaction.Name)));

                    var targetHost = new ActionHost(engine, target.Player);
                    var reaction = await potentialReaction.ExecuteReactionAsync(targetHost);          
                    if (reaction.Type == ReactionType.Replace)
                    {
                        engine.LogPartialEvent($@"<spans>
                            <player>{target.Player}</player>
                            <if you='reveal' them='reveals'>{target.Player}</if>
                            <card suffix='!'>{potentialReaction.Name}</card>
                        </spans>");

                        replaced = true;
                        reaction.Enact(targetHost, this);
                    }
                }

                if (!replaced)
                {
                    await act(target);
                }
            }
        }
    }
}