using Cardgame.API;

namespace Cardgame.Cards.Alchemy
{
    public class Possession : ActionCardBase
    {
        public override Cost Cost => new Cost(6, true);

        public override string Text => @"<small>
            <run>The player to your left takes an extra turn after this one, in which you can see all cards they can and make all decisions for them. Any cards or</run>
            <sym>debt</sym>
            <run>they would gain on that turn, you gain instead; any cards of theirs that are trashed are set aside and put in their discard pile at end of turn.</run>
        </small>";

        protected override void Act(IActionHost host)
        {
            host.InsertExtraTurn(host.GetPlayerToLeft(), host.Player);
        }
    }
}