using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cardgame
{
    public class ActionHost : IActionHost
    {
        private readonly GameEngine engine;
        public string Player { get; }

        public ActionHost(GameEngine engine, string player)
        {
            this.engine = engine;
            this.Player = player;
        }

        private string Verb(string secondPerson, string thirdPerson, string continuous)
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

        private string CardList(string[] ids)
        {
            return string.Join(Environment.NewLine, ids.Select((id, ix) => 
            {
                var suffix = ix == ids.Length -1 ? "."
                    : ix < ids.Length - 2 ? ","
                    : " and";
                return $"<card suffix='{suffix}'>{id}</card>";
            }));
        }

        string[] IActionHost.GetHand()
        {
            return engine.Model.Hands[Player].ToArray();
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
                engine.LogPartialEvent($@"<spans>
                    <run>...</run>
                    <if you='(you reshuffle.)' them='(reshuffling.)'>{Player}</if>
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
                engine.DiscardCard(Player, card);
            }

            engine.LogPartialEvent($@"<spans>
                <run>...</run>
                {Verb("discard", "discards", "discarding")}
                {CardList(cards)}
            </spans>");
        }

        void IActionHost.TrashCards(string[] cards)
        {
            foreach (var card in cards)
            {
                engine.TrashCard(Player, card);
            }

            engine.LogPartialEvent($@"<spans>
                <run>...</run>
                {Verb("trash", "trashes", "trashing")}
                {CardList(cards)}
            </spans>");        
        }

        void IActionHost.GainCard(string id)
        {
            engine.GainCard(Player, id);

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
                CardSource.Hand => engine.Model.Hands[Player],
                CardSource.Kingdom => engine.Model.KingdomCards.Concat(new[]{"Estate", "Duchy", "Province", "Copper", "Silver", "Gold"}).Where(id => engine.Model.CardStacks[id] > 0),
                CardSource other => throw new CommandException($"Unknown CardSource {other}")
            };

            var filteredCards = filter(sourceCards.Select(id => Cards.All.ByName[id]));

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
                    .Select(id => Cards.All.ByName[id])
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