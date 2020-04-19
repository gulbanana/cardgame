using Cardgame.Shared;

namespace Cardgame.Cards.Intrigue
{
    public class BridgeEffect : EffectBase, IModifier
    {
        public override string Art => "int-bridge";
        public override string Text => @"
            <run>This turn, cards (everywhere) cost</run>
            <sym>coin1</sym>
            <run>less, but not less than</run>
            <sym suffix='.'>coin0</sym>
        ";

        public int ReduceCardCost => 1;
    }
}