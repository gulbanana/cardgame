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
            engine.MoveCard(Player, card, from, new Zone(ZoneName.Attached, Card));

            LogLine($@"
                {LogVerbInitial("put", "puts", "putting")}
                <run>a card under</run>
                <card suffix='.'>{Card.Id}</card>.
            ");
        }

        public override void Detach(Zone to)
        {
            engine.MoveCard(Player, null, new Zone(ZoneName.Attached, Card), to);

            LogLine($@"
                {LogVerbInitial("remove", "removes", "removing")}
                <run>a card from under</run>
                <card>{Card.Id}</card>
                <run>and</run>
                {LogDestination(to)}
            ");
        }
    }
}