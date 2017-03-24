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
                game.PlayerTurn(game.players.Peek());

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
