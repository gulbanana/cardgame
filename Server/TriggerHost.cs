using Cardgame.API;

namespace Cardgame.Server
{
    // for action sequences associated with a reaction trigger but not any particular source
    internal class TriggerHost : ActionHost
    {
        public Trigger TriggerType { get; }
        public string TriggerParameter { get; }

        public TriggerHost(GameEngine engine, int indentLevel, string owningPlayer, Trigger triggerType, string triggerParameter) : base(engine, indentLevel, owningPlayer)
        {
            TriggerType = triggerType;
            TriggerParameter = triggerParameter;
        }

        protected override IActionHost CloneHost(string owningPlayer)
        {
            return new TriggerHost(engine, IndentLevel, Player, TriggerType, TriggerParameter);
        }
    }
}