using Cardgame.API;
using Cardgame.Shared;

namespace Cardgame.Server
{
    // for action sequences caused by playing a card, whether as a reaction or otherwise
    internal class CardHost : ActionHost
    {
        public Instance Card { get; }

        public CardHost(GameEngine engine, int indentLevel, string owningPlayer, Instance sourceCard) : base(engine, indentLevel, owningPlayer)
        {
            Card = sourceCard;
        }

        protected override IActionHost CloneHost(string owningPlayer)
        {
            return new CardHost(engine, IndentLevel, owningPlayer, Card);
        }

        public override void CompleteDuration()
        {
            engine.IncompleteDurations.Remove(Card);
        }

        public override void Attach(string card, Zone from)
        {
            engine.MoveCard(Player, card, from, Zone.Attached(Card));

            engine.LogPartialEvent($@"<spans>
                <indent level='{IndentLevel}' />
                {LogVerbInitial("put", "puts", "putting")}
                <run>a card under</run>
                <card suffix='.'>{Card.Id}</card>.
            </spans>");
        }

        public override void Detach(Zone to)
        {
            engine.MoveCard(Player, null, Zone.Attached(Card), to);

            engine.LogPartialEvent($@"<spans>
                <indent level='{IndentLevel}' />
                {LogVerbInitial("remove", "removes", "removing")}
                <run>a card from under</run>
                <card>{Card.Id}</card>
                <run>and</run>
                {LogDestination(to)}
            </spans>");
        }
    }
}