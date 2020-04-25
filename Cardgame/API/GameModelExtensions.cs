using System;
using System.Linq;
using Cardgame.Shared;

namespace Cardgame.API
{
    public static class GameModelExtensions
    {        
        public static Cost MaxCost(this GameModel model)
        {
            return new Cost(model.CoinsRemaining, model.PotionsRemaining > 0);
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
    }
}