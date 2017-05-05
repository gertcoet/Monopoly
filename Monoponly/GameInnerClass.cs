using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections;

namespace Monoponly
{
    public partial class Game
    {
        #region publicClasses
        public class Player
        {
            public string name { get; set; }
            public int money { get; set; }
            public bool inJail { get; set; }
            public bool isSolvant { get; set; }
            public PlayerToker playerToken { get; set; }
            public BoardSpace currPos { get; set; }
            public BoardSpace prevPos { get; set; }
            private static Random rng = new Random();

            public event EventHandler<LandedOnSpaceEventArgs> LandOnSpace;


            static readonly int salary=200;
           
            public Player(string Name, PlayerToker PlayerToken, BoardSpace StartingPoint)
            {
                name = Name;
                money = 1500;
                inJail = false;
                isSolvant = true;
                playerToken = PlayerToken;
                currPos = StartingPoint;

                LandOnSpace += GameEventHandler.LandedOnSpaceHandler;
            }

            private void LogRollValues(Game game)
            {
                game.rollLog.Add(new RollLogEntry(this, game.dice));
            }

            public bool IsThirdDouble(Game game)
            {
                int count = 0;

                //check if there has been 3 rolls
                var playerEnrties = game.rollLog.FindAll(i => i.player == this);

                if (playerEnrties.Count < 3)
                    return false;

                var LastTreeEntries = playerEnrties.OrderByDescending(i => i.date).Take(3);

                //get last 3 and check doubles                

                foreach (RollLogEntry entry in LastTreeEntries)
                {
                    if (entry.IsMatch()) { count++; }
                }

                return count == 3 ? true : false;
            }
            public BoardSpace RollDiceAndMove(Game game)
            {
                game.dice.Roll();
                game.rollLog.Add(new RollLogEntry(this, game.dice));
                //Console.WriteLine(game.dice.ToString());

                //Move player to jail if this is his third double
                if (IsThirdDouble(game) && !inJail)
                {
                    inJail = true;
                    MovePlayer(game.Board[Jail], false);
                    game.players.NextPlayerTurn();
                    game.SendPlayerToJail(game.players.GetCurrecntPlayer(), "You have throw 3 doubles in a row - Go to Jail!");                    
                }

                //If player is in Jail remain in Jail
                if (inJail && !game.dice.IsMatch())
                {
                    game.players.NextPlayerTurn();
                    // TODO : raise player is in jail event
                    //throw new RemainInJailException($"{name} must remain in Jail!");
                }

                //Move player out of jail if in Jail. Move to new position and pay salary if needed
                if (game.dice.IsMatch() && inJail)
                {
                    MovePlayer(NewSpaceAfterRoll(game.dice, game), true);
                    inJail = false;
                    game.players.NextPlayerTurn();
                    return currPos;
                }

                //Normal roll, move player and pay salary as needed
                MovePlayer(NewSpaceAfterRoll(game.dice, game), true);
                if (!(game.dice.IsMatch())) { game.players.NextPlayerTurn(); }
                return currPos;
            }

            public int DeductMoney(int Amount)
            {
                if (Amount > money)
                    throw new InsufficientFundsException($"{name} doesn not have enough money to complete this transaction ({money}) ");

                return money = money-Amount;
            }

            public void GiveSalary()
            {
                money += salary;
            }
            public void MovePlayer(BoardSpace ToSpace, bool PassedGo)
            {
                //Check if the player must be paid for passing go
                if (PassedGo && (ToSpace.spaceNumber <= currPos.spaceNumber))
                    money += salary;

                prevPos = currPos;
                currPos = ToSpace;
            }

            public void MovePlayer(int SpacesToMove, bool PassedGo,Game game)
            {
                int index = currPos.spaceNumber + SpacesToMove;

                if (index < 0) //Bo back before go on board
                    index += game.Board.Count;

                if (index > game.Board.Count) //Passed go                
                    index = index % game.Board.Count;

                MovePlayer(game.Board[index], PassedGo);                
            }
            public BoardSpace NewSpaceAfterRoll(DiceSet dice, Game game)
            {
                int newPos = ((currPos.spaceNumber + dice.RollTotal()) % game.Board.Count);
                return game.Board[newPos];
            }

            public void PlayerLandedOnSpace(Game game, BoardSpace space)
            {

                if (LandOnSpace != null)
                    LandOnSpace(this, new LandedOnSpaceEventArgs { game = game, boardspace = space, player = this });
                //TODO hook upp the event actrion

            }

        }


        public class PlayerCollection : IEnumerable
        {
            private Queue<Player> players;

            public PlayerCollection()
            {
                players = new Queue<Player>();
            }

            public Player GetCurrecntPlayer()
            {
                return players.Peek();
            }

            public void NextPlayerTurn()
            {
                Player p = players.Dequeue();
                players.Enqueue(p);
            }

            public void Add(Player player)
            {
                //Check if there are any player in the que first
                if (players.Count > 0)
                {
                    //Check if player exist
                    var existing = players.FirstOrDefault(p => p.name == player.name);
                    if (existing != null)
                        throw new PlayerAlreadyExistsException($"{player.name} already is already registered for this game");
                }

                //Add player
                players.Enqueue(player);

            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return players.GetEnumerator();
            }
        }

      

        public class DiceSet
        {
            Random rand;
            public int dice1 { get; set; }
            public int dice2 { get; set; }

            public DiceSet()
            {
                rand = new Random();
            }

            public void Roll()
            {
                dice1 = rand.Next(1, 6);
                dice2 = rand.Next(1, 6);
            }

            public Boolean IsMatch()
            {
                return dice1 == dice2;
            }

            public int RollTotal()
            {
                return dice1 + dice2;
            }

            public override string ToString()
            {
                return $"Dice1 : {dice1} ; Dice2 : {dice2}";
            }
        }

        public class RollLogEntry
        {
            public Player player { get; set; }
            public int dice1 { get; set; }
            public int dice2 { get; set; }
            public DateTime date { get; set; }

            public RollLogEntry(Player player, DiceSet dice)
            {
                this.player = player;
                this.dice1 = dice.dice1;
                this.dice2 = dice.dice2;
                date = System.DateTime.UtcNow;
            }

            public bool IsMatch()
            {
                return dice1 == dice2 ? true : false;
            }
        }

      
     

        #endregion
    }
}
