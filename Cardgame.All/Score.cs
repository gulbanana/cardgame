namespace Cardgame.All
{
    public class Score
    {    
        public string Player { get; set; }
        public int Total { get; set; }
        public (string card, int count, int subtotal)[] Subtotals { get; set; }
    }
}