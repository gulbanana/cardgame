using Cardgame.Engine;
using Cardgame.Model;
using System;
using System.Linq;

namespace Cardgame.Tests
{
    public abstract class TestBase
    {
        protected readonly GameEngine engine;
        protected readonly GameModel model;

        protected string us { get; private set; }
        protected string them { get; private set; }

        static TestBase()
        {
            All.Cards.Init(typeof(Cards.CardBase).Assembly);
            All.Effects.Init(typeof(Cards.EffectBase).Assembly);
            All.Mats.Init(typeof(Cards.MatBase).Assembly);
        }

        public TestBase()
        {
            engine = new GameEngine();
            model = engine.Model;

            engine.ExecuteInternal("player1", new JoinGameCommand());
            engine.ExecuteInternal("player2", new JoinGameCommand());
            engine.ExecuteInternal("player1", new StartGameCommand());

            us = engine.Model.ControllingPlayer;
            them = engine.Model.ControllingPlayer == "player1" ? "player2" : "player1";
        }

        protected void AddCard(string id, string player = null)
        {
            if (player == null) player = model.ActivePlayer;
            if (player == model.ActivePlayer)
            {
                if (model.CurrentPhase > Phase.Action) model.CurrentPhase = Phase.Action;
                if (model.ActionsRemaining < 1) model.ActionsRemaining = 1;
            }

            if (!model.Supply.ContainsKey(id))
            {
                model.Supply[id] = 10;
                model.SupplyTokens[id] = new string[0];
            }

            if (!model.Hands[player].Names().Contains(id))
            {
                model.Hands[player].Add(Instance.Of(id));
            }
        }
    }
}
