using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cardgame
{
    public class ActionHost : IActionHost
    {
        private readonly GameEngine engine;
        private readonly string player;

        public ActionHost(GameEngine engine, string player)
        {
            this.engine = engine;
            this.player = player;
        }

        private string Verb(string secondPerson, string thirdPerson, string continuous)
        {
            if (player == engine.Model.ActivePlayer)
            {
                return $"<if you='you {secondPerson}' them='{continuous}'>{player}</if>";
            }
            else
            {
                return $"<player>{player}</player><if you='{secondPerson}' them='{thirdPerson}'>{player}</if>";
            }
        }

        int IActionHost.GetHandCards()
        {
            return engine.Model.Hands[player].Count;
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
                reshuffled = reshuffled | engine.DrawCard(player);
            }
            if (reshuffled)
            {
                engine.LogPartialEvent($@"<spans>
                    <run>...</run>
                    <if you='(you reshuffle.)' them='(reshuffling.)'>{player}</if>
                </spans>");
            }

            engine.LogPartialEvent($@"<spans>
                <run>...</run>
                {Verb("draw", "draws", "drawing")}
                <run>{(n == 1 ? "a card." : $"{n} cards.")}</run>
            </spans>");
        }

        void IActionHost.DiscardCards(string[] cards)
        {
            foreach (var card in cards)
            {
                engine.DiscardCard(player, card);
            }

            var eventText = TextModel.Parse($@"<spans>
                <run>...</run>
                {Verb("discard", "discards", "discarding")}                
            </spans>") as TextModel.Spans;

            eventText.Children = eventText.Children.Concat(cards.Select((card, ix) => new TextModel.Card
            {
                Name = card,
                Suffix = ix == cards.Length -1 ? "."
                    : ix < cards.Length - 2 ? ","
                    : " and"
            })).ToArray();

            engine.LogPartialEvent(eventText);
        }

        void IActionHost.TrashCards(string[] cards)
        {
            foreach (var card in cards)
            {
                engine.TrashCard(player, card);
            }

            var eventText = TextModel.Parse($@"<spans>
                <run>...</run>
                {Verb("trash", "trashes", "trashing")}             
            </spans>") as TextModel.Spans;

            eventText.Children = eventText.Children.Concat(cards.Select((card, ix) => new TextModel.Card
            {
                Name = card,
                Suffix = ix == cards.Length -1 ? "."
                    : ix < cards.Length - 2 ? ","
                    : " and"
            })).ToArray();

            engine.LogPartialEvent(eventText);        }

        void IActionHost.GainCard(string id)
        {
            engine.GainCard(player, id);

            engine.LogPartialEvent($@"<spans>
                <run>...</run>
                {Verb("gain", "gains", "gaining")}
                <card suffix='.'>{id}</card>
            </spans>");
        }

        async Task<T> IActionHost.SelectCard<T>(string prompt, CardSource source, Func<IEnumerable<Cards.CardModel>, IEnumerable<T>> filter)
        {
            var sourceCards = source switch 
            {
                CardSource.Hand => engine.Model.Hands[player],
                CardSource.Kingdom => engine.Model.KingdomCards.Concat(new[]{"Estate", "Duchy", "Province", "Copper", "Silver", "Gold"}).Where(id => engine.Model.CardStacks[id] > 0),
                CardSource other => throw new CommandException($"Unknown CardSource {other}")
            };

            var filteredCards = filter(sourceCards.Select(id => Cards.All.ByName[id]));

            var id = await engine.Choose<string[], string>(
                player,
                ChoiceType.SelectCard, 
                $"<run>{prompt}</run>", 
                filteredCards.Select(card => card.Name).ToArray()
            );

            return filteredCards.First(card => card.Name == id);
        }

        Task<string[]> IActionHost.SelectCardsFromHand(string prompt, int? number)
        {
            return engine.Choose<SelectCardsInput, string[]>(
                player,
                ChoiceType.SelectCards, 
                $"<run>{prompt}</run>",
                new SelectCardsInput
                {
                    Choices = engine.Model.Hands[player].ToArray(),
                    NumberRequired = number
                }
            );
        }

        async Task IActionHost.Attack(Func<IActionHost, bool> filter, Func<IActionHost, Task> act)
        {
            var targetPlayers = engine.Model.Players
                .Except(new[]{ player })
                .Select(player => new ActionHost(engine, player))
                .Where(filter)
                .ToList();
            
            foreach (var player in targetPlayers)
            {
                await act(player);
            }
        }
    }
}