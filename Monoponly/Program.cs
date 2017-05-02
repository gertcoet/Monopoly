using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monoponly
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {

                Game game = new Game();
                
                
                foreach(Game.CCard card in game.comChest)
                {
                    Game.Player p = game.players.GetCurrecntPlayer();
                    Console.WriteLine(card.description);
                    card.PerformActions(game, p);
                    Console.WriteLine($"space={p.currPos.name} : {p.money}");
                }
                
                
                //for (int k = 0; k <= 1000; k++)
                //{
                //    game.PlayerTurn(p);
                //}

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
