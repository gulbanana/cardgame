namespace Cardgame.Model
{
    public class SelectCardsInput
    {
        public string[] Choices { get; set; }
        public int? Min { get; set; }
        public int? Max { get; set; }
    }
}