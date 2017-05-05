using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monoponly
{
    public partial class Game 
    {
        public class GameEventHandler
        {

            public static void SentToJailHandler(object sender, SendToJailEventArgs e)
            {
                Console.WriteLine($"*** {e.player.name} is going to Jail***");
                Console.WriteLine(e.message);
                Console.WriteLine("Do you want to pay R50 and get out of Jail immediately?");
                string ans = Console.ReadLine();

                /*** Move the player to the Jail position and set jail status as reiquired
                by his option ***/
                e.player.MovePlayer(e.jail,false);
                if (ans.ToUpper() != "Y")
                {
                    e.player.inJail = true;        
                }
                else
                {
                    e.player.DeductMoney(50);
                }

                Console.WriteLine($"*** {e.player.name} --> Your turn has ended ***" + System.Environment.NewLine);
                e.game.players.NextPlayerTurn();                                
                Console.WriteLine($"*** {e.game.players.GetCurrecntPlayer().name} --> You're turn" + System.Environment.NewLine);
            }

        }
        public class SendToJailEventArgs : EventArgs
        {
            public Player player { get; set; }
            public string message { get; set; }
            public BoardSpace jail { get; set; }
            public Game game { get; set; }
        }

    }

}
