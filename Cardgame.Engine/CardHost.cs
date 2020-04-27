using Cardgame.API;
using Cardgame.Engine.Logging;
using Cardgame.Model;

namespace Cardgame.Engine
{
    // for action sequences caused by playing a card, whether as a reaction or otherwise
    internal class CardHost : ActionHost
    {
        public Instance Card { get; }

        public CardHost(GameEngine engine, IRecord logRecord, string owningPlayer, Instance sourceCard) : base(engine, logRecord, owningPlayer)
        {
            Card = sourceCard;
        }

        protected override IActionHost CloneHost(IRecord logRecord, string owningPlayer)
        {
            return new CardHost(engine, logRecord, owningPlayer, Card);
        }

        public override void CompleteDuration()
        {
            engine.Model.PlayedWithDuration.Remove(Card);
        }

        public override void Attach(string card, Zone from)
        {
            var attachee = new Zone(ZoneName.Attached, Card);
            engine.MoveCard(Player, card, from, attachee);

            LogMovement(Motion.Put, new[]{card}, from, attachee);
        }

        public override void Detach(Zone to)
        {
            var attachee = new Zone(ZoneName.Attached, Card);
            var attachment = engine.MoveCard(Player, null, attachee, to);

            LogMovement(Motion.Put, new[]{ attachment.Id }, attachee, to);
        }
    }
}