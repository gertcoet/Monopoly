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
            public static void PlayerTurnEnded(Game game)
            {
                Console.WriteLine($"*** {game.players.GetCurrecntPlayer().name} --> Your turn has ended ***" + System.Environment.NewLine);
                game.players.NextPlayerTurn();
                Console.WriteLine($"*** {game.players.GetCurrecntPlayer().name} --> You're turn" + System.Environment.NewLine);
            }
            
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

                GameEventHandler.PlayerTurnEnded(e.game);                
            }

            public static void DoPlayerWantToBuy(PurchasableBoardSpace property,Player player)
            {
                Console.WriteLine(property.GetProperyDetails());
                Console.WriteLine($"{player.name} --> Do you want to buy {property.name}?");

                string input = Console.ReadLine();

                if (input.ToUpper() == "Y" || input.ToUpper() == "YES")
                {
                    player.DeductMoney(property.purchasePrice);
                    property.owner = player;
                    Console.WriteLine($"Congradulations {player.name}! You now own {property.name}");
                }

                // TODO: fire an autction
            }

            public static void LandedOnSpaceHandler(object sender, LandedOnSpaceEventArgs e)
            {
                BoardSpace space = e.boardspace;

                if (space is PurchasableBoardSpace property)
                {
                    Console.WriteLine($"{e.player.name} --> landed on ** {property.name} **");

                    if (property.owner == null) //No owner
                    {
                        //Check if the player wants to buy the property
                        DoPlayerWantToBuy(property, e.player);
                    }
                    else //Has owner
                    {
                        int rent = property.RentToPay(e.game, e.player);

                        if (rent > 0)
                        {
                            e.player.DeductMoney(rent);
                            Console.WriteLine($"Paid --> R{rent} to {property.owner.name}, ownder of {property.name}");
                        }
                        else
                        {
                            Console.WriteLine($"No monies are due");
                        }
                    }
                }
       
                if (space is Corner corner)
                {
                    switch (corner.cornerType)
                    {
                        case CornerType.FreeParking:
                            Console.WriteLine($"{e.player.name} --> landed on ** {corner.name} **");
                            return;

                        case CornerType.Jail:
                            Console.WriteLine($"{e.player.name} --> landed on ** {corner.name} **");
                            return;

                        case CornerType.Start:
                            Console.WriteLine($"{e.player.name} --> landed on ** {corner.name} **");
                            return;
                    }

                }

                if (space is Chance chance)
                {
                    Console.WriteLine($"{e.player.name} --> landed on ** {chance.name} **");
                    // TODO : impliment chance actions
                }

                if (space is Tax tax)
                {
                    Console.WriteLine($"{e.player.name} --> landed on ** {tax.name} **");
                    // TODO : impliment tax actions
                }
            }                  
        }

        public class SendToJailEventArgs : EventArgs
        {
            public Player player { get; set; }
            public string message { get; set; }
            public BoardSpace jail { get; set; }
            public Game game { get; set; }
        }

        public class LandedOnSpaceEventArgs : EventArgs
        {
            public Player player { get; set; }            
            public BoardSpace boardspace { get; set; }
            public Game game { get; set; }
        }

    }

}
