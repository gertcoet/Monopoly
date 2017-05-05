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
                Console.WriteLine($"{e.player.name} has been sent to Jail!");
                Console.WriteLine("Do you want to pay R50 to get out of Jail now?");
                string ans = Console.ReadLine();

                //Move the player to the Jail position and set jail status as reiquired
                //by his option
                e.player.MovePlayer(e.jail,false);
                if (ans.ToUpper() != "Y")
                {
                    e.player.inJail = true;        
                }
                else
                {
                    e.player.DeductMoney(50);
                }

            }

        }
        public class SendToJailEventArgs : EventArgs
        {
            public Player player { get; set; }
            public string message { get; set; }

            public BoardSpace jail { get; set; }
        }

    }

}
