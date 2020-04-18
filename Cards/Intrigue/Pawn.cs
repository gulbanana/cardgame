using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Intrigue
{
    public class Pawn : ActionCardBase
    {
        public override string Art => "int-pawn";
        public override int Cost => 2;

        public override string Text => @"<lines>
            <spans>        
                <run>Choose two:</run>
                <block><run>+1 Card;</run></block>
                <block><run>+1 Action;</run></block>
                <block><run>+1 Buy;</run></block>
                <sym prefix='+' suffix='.'>coin1</sym>
            </spans>
            <run>The choices must be different.</run>
        </lines>";

        protected override async Task ActAsync(IActionHost host)
        {
            bool draw, act, buy, money;
            draw = act = buy = money = false;

            var options = new List<(string, Action)>
            {
                ("+1 Card", () => draw = true),
                ("+1 Action", () => act = true),
                ("+1 Buy", () => buy = true),
                ("+1 Money", () => money = true)
            };

            await host.ChooseOne("Pawn", options);

            if (draw) options.RemoveAt(0);
            if (act) options.RemoveAt(1);
            if (buy) options.RemoveAt(2);
            if (money) options.RemoveAt(3);

            await host.ChooseOne("Pawn", options);

            if (draw) host.DrawCards(1);
            if (act) host.AddActions(1);
            if (buy) host.AddBuys(1);
            if (money) host.AddMoney(1);
        }
    }
}