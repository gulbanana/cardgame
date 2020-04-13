namespace Cardgame
{
    public interface IActionHost
    {
        void DrawCards(int n);
        void AddActions(int n);
        void AddBuys(int n);
        void AddMoney(int n);
    }
}