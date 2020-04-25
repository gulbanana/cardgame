using System;
using System.Linq;
using Cardgame.API;
using Cardgame.Model;

namespace Cardgame.All
{
    // XXX these functions use types from both Shared and API, and are needed both by Client and by Server; for a small amount of shared code we have a lot of coupling
    public static class GameModelExtensions
    {        
        public static Cost MaxCost(this GameModel model)
        {
            return new Cost(model.CoinsRemaining, model.PotionsRemaining > 0);
        }

        public static int GetInitialSupply(this GameModel model, string card)
        {
            var victoryCount = model.Players.Length == 2 ? 8 : 12;
            return card switch
            {
                "Copper" => 60 - (model.Players.Length * 7),
                "Silver" => 40,
                "Gold" => 30,
                "Curse" => (model.Players.Length - 1) * 10,
                "Potion" => 16,
                string id => All.Cards.ByName(id).Types.Contains(API.CardType.Victory) ? victoryCount : 10
            };
        }

        public static IModifier[] GetModifiers(this GameModel model)
        {
            if (!model.IsStarted)
            {
                return Array.Empty<IModifier>();
            }
            else
            {
                var fx = model.ActiveEffects.Select(All.Effects.ByName).OfType<IModifier>();
                var inPlay = model.PlayedCards.Values.SelectMany(instances => instances).Select(All.Cards.ByName).OfType<IModifier>();

                return fx.Concat(inPlay).ToArray();
            }
        }

        public static Cost GetCost(this ICard card, GameModel modifierSource) => card.GetCost(modifierSource?.GetModifiers() ?? Array.Empty<IModifier>());

        public static Cost? GetModifiedValue(this ITreasureCard card, GameModel modifierSource) => card.GetModifiedValue(modifierSource.GetModifiers());        
    }
}