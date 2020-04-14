using System.Threading.Tasks;

namespace Cardgame
{
    public interface IActionHost
    {
        void DrawCards(int n);
        void AddActions(int n);
        void AddBuys(int n);
        void AddMoney(int n);

        void DiscardCards(string[] cards);

        Task<string[]> SelectCardsFromHand(string prompt);        
    }
}