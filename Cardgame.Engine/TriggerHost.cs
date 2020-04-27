using Cardgame.API;
using Cardgame.Engine.Logging;

namespace Cardgame.Engine
{
    // for action sequences associated with a reaction trigger but not any particular source
    internal class TriggerHost : ActionHost
    {
        public Trigger TriggerType { get; }
        public string TriggerParameter { get; }

        public TriggerHost(GameEngine engine, IRecord logRecord, string owningPlayer, Trigger triggerType, string triggerParameter) : base(engine, logRecord, owningPlayer)
        {
            TriggerType = triggerType;
            TriggerParameter = triggerParameter;
        }

        protected override IActionHost CloneHost(IRecord logRecord, string owningPlayer)
        {
            return new TriggerHost(engine, logRecord, owningPlayer, TriggerType, TriggerParameter);
        }
    }
}