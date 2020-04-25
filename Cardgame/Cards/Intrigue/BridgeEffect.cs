namespace Cardgame.Cards.Intrigue
{
    public class BridgeEffect : ModifierEffectBase
    {
        public override string Art => "int-bridge";
        public override string Text => @"
            <run>This turn, cards (everywhere) cost</run>
            <sym>coin1</sym>
            <run>less, but not less than</run>
            <sym suffix='.'>coin0</sym>
        ";

        public override int ReduceCardCost => 1;
    }
}