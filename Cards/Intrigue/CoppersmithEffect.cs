namespace Cardgame.Cards.Intrigue
{
    public class CoppersmithEffect : ModifierEffectBase
    {
        public override string Art => "int-coppersmith";
        public override string Text => @"<spans>
            <run>Copper produces an extra</run>
            <sym>coin1</sym>
            <run>this turn.</run>
        </spans>";

        public override int IncreaseTreasureValue(string id) => id == "Copper" ? 1 : 0;
    }
}