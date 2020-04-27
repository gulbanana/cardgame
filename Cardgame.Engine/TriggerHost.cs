using Cardgame.API;

namespace Cardgame.Engine
{
    // for action sequences associated with a reaction trigger but not any particular source
    internal class TriggerHost : ActionHost
    {
        public Trigger TriggerType { get; }
        public string TriggerParameter { get; }

        public TriggerHost(GameEngine engine, LogRecord logRecord, int indentLevel, string owningPlayer, Trigger triggerType, string triggerParameter) : base(engine, logRecord, indentLevel, owningPlayer)
        {
            TriggerType = triggerType;
            TriggerParameter = triggerParameter;
        }

        protected override IActionHost CloneHost(string owningPlayer)
        {
            return new TriggerHost(engine, logRecord, IndentLevel, owningPlayer, TriggerType, TriggerParameter);
        }
    }
}