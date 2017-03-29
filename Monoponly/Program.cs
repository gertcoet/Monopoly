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
                Game.Player p = game.players.Peek();
                for (int k = 0; k <= 1000; k++)
                {
                    game.PlayerTurn(p);                    
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
