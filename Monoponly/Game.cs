using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace Monoponly
{
    public class Game
    {
        public int houses { get; set; }
        public int hotels { get; set; }
        public DiceSet dice = new DiceSet();
        public List<BoardSpace> Board = new List<BoardSpace>();
        public Queue<Player> players = new Queue<Player>();

        public const string Jail = "Jail";

        #region enum
        public enum PlayerToker
        {
            Dog,
            Car,
            Ship,
            Plane,
            Hat,
            Cat,
            Boot,
            Wheelbarrow,
            thimble
        }

        public enum CornerType
        {
            Start,
            Jail,
            FreeParking,
            GoToJail
        }

        public enum StreetColour
        {
            Purple,
            Blue,
            Pink,
            Organe,
            Red,
            Yellow,
            Green,
            LightBlue,
            White,
            Brown
        }

        public enum ChanceType
        {
            Chance,
            CommunityChest
        }
        #endregion

        #region publicClasses
        public class Player
        {
            public string name { get; set; }
            public int money { get; set; }
            public bool inJail { get; set; }
            public bool isSolvant { get; set; }
            public PlayerToker playerToken { get; set; }
            public List<DiceSet> rollLog { get; set; }
            public BoardSpace currPos { get; set; }
            public BoardSpace prevPos { get; set; }

            static readonly int salary;

            public Player(string Name,PlayerToker PlayerToken,BoardSpace StartingPoint)
            {
                name = Name;
                money = 1500;
                inJail = false;
                isSolvant = true;
                playerToken = PlayerToken;
                currPos = StartingPoint;
                rollLog = new List<DiceSet>();
            }
            private void LogRollValues(DiceSet dice)
            {
                rollLog.Add(dice);
            }
            public bool IsThirdDouble()
            {
                //There has not been 3 rolls
                if (rollLog.Count < 3)
                        return false;

                for(int k = rollLog.Count;k >= rollLog.Count - 3; k--)
                {
                    if (!(rollLog[k].IsMatch()))
                        return false;                    
                }

                return true;
            }
            public BoardSpace RollDiceAndMove(Game game)
            {
                DiceSet dice = new DiceSet();
                dice.Roll();
                rollLog.Add(dice);

                //Move player to jail if this is his third double
                if (IsThirdDouble() && !inJail)
                {
                    inJail = true;
                    MovePlayer(game.GetSpaceByName(game,Jail),false);
                    TurnOfNextPlayer(game);
                    throw new GoToJailException($"{name} has been sent to Jail!");                    
                }

                //If player is in Jail remain in Jail
                if (inJail && !dice.IsMatch())
                {
                    TurnOfNextPlayer(game);
                    throw new RemainInJailException($"{name} must remain in Jail!");
                }

                //Move player out of jail if in Jail. Move to new position and pay salary if needed
                if (dice.IsMatch() && inJail)
                {
                    MovePlayer(NewSpaceAfterRoll(dice, game),true);
                    inJail = false;
                    TurnOfNextPlayer(game);
                    return currPos;
                }

                //Normal roll, move player and pay salary as needed
                MovePlayer(NewSpaceAfterRoll(dice, game), true);
                if (!(dice.IsMatch())) { TurnOfNextPlayer(game); }
                return currPos;                
            }
            public int DeductMoney(int Amount)
            {
                if (Amount > money)
                    throw new InsufficientFundsException($"{name} doesn not have enough money to complete this transaction ({money}) ");

                return money=-Amount;
            }
            public void GiveSalary()
            {
                money += salary;
            }
            public void MovePlayer(BoardSpace ToSpace,bool PassedGo)
            {
                //Check if the player must be paid for passing go
                if (PassedGo && (ToSpace.spaceNumber < currPos.spaceNumber))
                    money += salary;

                prevPos = currPos;
                currPos = ToSpace;                
            }
            public BoardSpace NewSpaceAfterRoll(DiceSet dice,Game game)
            {
                int newPos = ((currPos.spaceNumber + dice.RollTotal()) % game.Board.Count);
                return game.Board[newPos];
            }

            public void TurnOfNextPlayer(Game game)
            {
                Player p = game.players.Dequeue();
                game.players.Enqueue(p);

            }
        }

        public abstract class BoardSpace
        {
            public string name { get;  }
            public int spaceNumber { get; }            

            public BoardSpace(String Name, int SpaceNumber)
            {
                this.name = Name;                
                this.spaceNumber = SpaceNumber;
            }
            public override string ToString()
            {
                return JsonConvert.SerializeObject(this);
            }
        }

        public abstract class PurchasableBoardSpace : BoardSpace
        {
            public int purchasePrice { get; }
            public int mortageValue { get; }
            public bool isMortaged { get; set; }
            public int[] rent { get; }
            public Player owner { get; set; }

            public PurchasableBoardSpace(string Name, int SeqenceNumber, int PurhcasePrice, int MortageValue) : base(Name, SeqenceNumber)
            {
                this.purchasePrice = PurhcasePrice;
                this.mortageValue = MortageValue;
                this.isMortaged = false;
            }
        }

        public class Corner : BoardSpace
        {
            public CornerType corner { get; }
            public Corner(string Name, int SeqenceNumber, CornerType Corner) : base(Name, SeqenceNumber)
            {
                this.corner = corner;
            }
        }

        public class Tax : BoardSpace
        {
            public int taxAmount { get; }
            public int totalWorthPerc { get; }

            public Tax(string Name, int SeqenceNumber, int Tax, int TotalWorthPerc ) : base(Name, SeqenceNumber)
            {
                this.taxAmount = Tax;
                this.totalWorthPerc = TotalWorthPerc;
            }
            

        }

        public class Chance : BoardSpace
        {
            public ChanceType chaneType { get; set; }

            public Chance(string Name, int SeqenceNumber , ChanceType ChanceType) : base(Name, SeqenceNumber)
            {
                this.chaneType = ChanceType;
            }
        }

        public class Transportation : PurchasableBoardSpace
        {
            public int oneRR { get; set; }
            public int twoRR { get; set; }
            public int threeRR { get; set; }
            public int fourRR { get; set; }

            public Transportation(string Name, int SeqenceNumber, int PurhcasePrice, int MortageValue , int OneRR, int TwoRR, int ThreeRR, int FourRR) : base(Name, SeqenceNumber, PurhcasePrice, MortageValue)
            {
                this.oneRR = OneRR;
                this.twoRR = TwoRR;
                this.threeRR = ThreeRR;
                this.fourRR = FourRR;
            }
        }

        public class Utility : PurchasableBoardSpace
        {
            public int oneUtility { get;  }
            public int twoUtlility { get;  }
            public Utility(string Name, int SeqenceNumber, int PurhcasePrice, int MortageValue, int OneUtility, int TwoUtlility) : base(Name, SeqenceNumber, PurhcasePrice, MortageValue)
            {
                this.oneUtility = OneUtility;
                this.twoUtlility = TwoUtlility;
            }
        }

        public class Property : PurchasableBoardSpace
        {
            public int streetInColour { get; set; }
            public int buildingCost { get; set; }
            public StreetColour streetColour { get; set; }
            public int buildingsOnProperty { get; set; }

            public int rent { get; }
            public int house1 { get; }
            public int house2 { get; }
            public int house3 { get; }
            public int house4 { get; }
            public int hotel { get; }

            public Property(string Name, int SeqenceNumber, int PurhcasePrice, int MortageValue, int Rent, int House1, int House2, int House3, int House4, int Hotel) : base(Name, SeqenceNumber, PurhcasePrice, MortageValue)
            {
                this.rent = Rent;
                this.house1 = House1;
                this.house2 = House2;
                this.house3 = House3;
                this.house4 = House4;
                this.hotel = Hotel;
                this.buildingsOnProperty = 0;
            }


        }

        public class DiceSet
        {
            public int dice1 { get; set; }
            public int dice2 { get; set; }

            public void Roll()
            {
                dice1 = GameUtilities.RollDice(6);
                dice2 = GameUtilities.RollDice(6);
            }

            public Boolean IsMatch()
            {
                return dice1 == dice2;                    
            }

            public int RollTotal()
            {
                return dice1 + dice2;
            }
        }
        #endregion

        #region Exceptions
        public class InsufficientFundsException : Exception
        {
            public InsufficientFundsException()
            {
            }

            public InsufficientFundsException(string message) : base(message)
            {
            }
        }

        public class GoToJailException: Exception
        {
            public GoToJailException()
            {
            }

            public GoToJailException(string message) : base(message)
            {
            }
        }

        public class RemainInJailException : Exception
        {
            public RemainInJailException()
            {
            }

            public RemainInJailException(string message) : base(message)
            {
            }
        }

        #endregion
        public override string ToString()
        {           
            return JsonConvert.SerializeObject(this);            
        }

        public Game()
        {
            // First block
            Board.Add(new Corner("Go", 0, CornerType.Start));
            Board.Add(new Property("Old Kent Road", 1, 60, 2, 30, 10, 30, 90, 160, 250) { streetInColour = 2, streetColour = StreetColour.Brown });
            Board.Add(new Chance("Community Chest", 2, ChanceType.CommunityChest));
            Board.Add(new Property("Whitechapel Road", 3, 60, 30, 4, 20, 60, 180, 320, 450) { streetInColour = 2, streetColour = StreetColour.Brown });
            Board.Add(new Tax("Income Tax", 4, 200, 10));
            Board.Add(new Transportation("Kings Cross Station", 5, 200, 100, 25, 50, 100, 200));
            Board.Add(new Property("The Angel Islington", 6, 100, 50, 6, 30, 90, 270, 400, 550) { streetInColour = 3, streetColour = StreetColour.LightBlue });
            Board.Add(new Chance("Chance", 7, ChanceType.Chance));
            Board.Add(new Property("Euston Road", 8, 100, 50, 6, 30, 90, 270, 400, 550) { streetInColour = 3, streetColour = StreetColour.LightBlue });
            Board.Add(new Property("Pentonville Road", 9, 120, 60, 8, 40, 100, 300, 450, 600) { streetInColour = 3, streetColour = StreetColour.LightBlue });
            // second block
            Board.Add(new Corner(Jail, 10, CornerType.Jail));
            Board.Add(new Property("Pall Mall", 11, 140, 70, 10, 50, 150, 450, 625, 750) { streetInColour = 3, streetColour = StreetColour.Pink });
            Board.Add(new Utility("Power Utility", 12, 150, 75, 4, 10));
            Board.Add(new Property("White Hall", 13, 140, 70, 10, 50, 150, 450, 625, 750) { streetInColour = 3, streetColour = StreetColour.Pink });
            Board.Add(new Property("Northumberland Avenue", 14, 160, 80, 12, 60, 180, 500, 700, 900) { streetInColour = 3, streetColour = StreetColour.Pink });
            Board.Add(new Transportation("Marylebone Station", 15, 200, 100, 25, 50, 100, 200));
            Board.Add(new Property("Bow Street", 16, 180, 90, 14, 70, 200, 550, 750, 950) { streetInColour = 3, streetColour = StreetColour.Organe, buildingCost = 100 });
            Board.Add(new Chance("Community Chest", 17, ChanceType.Chance));
            Board.Add(new Property("Marlborough Street", 18, 180, 90, 14, 70, 200, 550, 750, 950) { streetInColour = 3, streetColour = StreetColour.Organe, buildingCost = 100 });
            Board.Add(new Property("Vine Street", 19, 200, 100, 16, 80, 220, 600, 800, 1000) { streetInColour = 3, streetColour = StreetColour.Organe, buildingCost = 100 });
            // Third block
            Board.Add(new Corner("Free Parking", 20, CornerType.FreeParking));
            Board.Add(new Property("The Strand", 21, 220, 110, 18, 90, 250, 700, 875, 1050) { streetInColour = 3, streetColour = StreetColour.Red, buildingCost = 150 });
            Board.Add(new Chance("Chance", 22, ChanceType.Chance));
            Board.Add(new Property("Fleet Street", 23, 220, 110, 18, 90, 250, 700, 875, 1050) { streetInColour = 3, streetColour = StreetColour.Red, buildingCost = 150 });
            Board.Add(new Property("Trafalgar Square", 24, 240, 120, 20, 100, 300, 750, 925, 1100) { streetInColour = 3, streetColour = StreetColour.Red, buildingCost = 150 });
            Board.Add(new Transportation("Fenchurch St Station", 25, 200, 100, 25, 50, 100, 200));
            Board.Add(new Property("Leicester Square", 26, 260, 130, 22, 110, 330, 800, 975, 1150) { streetInColour = 3, streetColour = StreetColour.Yellow, buildingCost = 150 });
            Board.Add(new Property("Coventry Street", 27, 260, 130, 22, 110, 330, 800, 975, 1150) { streetInColour = 3, streetColour = StreetColour.Yellow, buildingCost = 150 });
            Board.Add(new Utility("Water Utility", 28, 150, 75, 4, 10));
            Board.Add(new Property("Piccadilly", 29, 280, 140, 22, 120, 360, 850, 1025, 1200) { streetInColour = 3, streetColour = StreetColour.Yellow, buildingCost = 150 });
            // Fourth block
            Board.Add(new Corner("Go To Jail", 30, CornerType.GoToJail));
            Board.Add(new Property("Regent Street", 31, 300, 150, 26, 130, 390, 900, 1100, 1275) { streetInColour = 3, streetColour = StreetColour.Green, buildingCost = 200 });
            Board.Add(new Property("Oxford Street", 32, 300, 150, 26, 130, 390, 900, 1100, 1275) { streetInColour = 3, streetColour = StreetColour.Green, buildingCost = 200 });
            Board.Add(new Chance("Community Chest", 33, ChanceType.CommunityChest));
            Board.Add(new Property("Bond Street", 34, 320, 160, 28, 150, 450, 1000, 1200, 1400) { streetInColour = 3, streetColour = StreetColour.Green, buildingCost = 200 });
            Board.Add(new Transportation("Liverpool Street Station", 35, 200, 100, 25, 50, 100, 200));
            Board.Add(new Chance("Chance", 36, ChanceType.Chance));
            Board.Add(new Property("Park Lane", 37, 350, 175, 35, 175, 500, 1100, 1300, 1500) { streetInColour = 2, streetColour = StreetColour.Blue, buildingCost = 200 });
            Board.Add(new Tax("Luxury Tax", 38, 100, 0));
            Board.Add(new Property("Mayfair", 39, 400, 200, 50, 200, 600, 1400, 1700, 2000) { streetInColour = 2, streetColour = StreetColour.Blue, buildingCost = 200 });
            //Add Players
            players.Enqueue(new Player("Player One", PlayerToker.Boot,GetSpaceByName(this,"Go")));
            players.Enqueue(new Player("Player Two", PlayerToker.Car, GetSpaceByName(this, "Go")));
            players.Enqueue(new Player("Player Three", PlayerToker.Cat, GetSpaceByName(this, "Go")));
            players.Enqueue(new Player("Player Four", PlayerToker.Dog, GetSpaceByName(this, "Go")));

        }

        public bool IsWinner()
        {
            int count = 0;

            foreach (Player p in players)
            {                
                if (p.isSolvant)
                    count++;
            }

            //If there is only one solvent player the game is over
            return count == 1? true : false;
        }

        public BoardSpace GetSpaceByName(Game game, string name)
        {
            return game.Board.Find(item => item.name == name);
        }

        public void PlayerTurn(Player player)
        {
            try
            {
                Console.WriteLine(player.currPos.name);
                player.RollDiceAndMove(this);
                Console.WriteLine(player.currPos.name);
            }
            catch (Exception)
            {
                throw;
            }
            
        }

        public string printOwnable()
        {
            string json = string.Empty;

            foreach(BoardSpace space in this.Board)
            {
                if (space is PurchasableBoardSpace)
                {
                   json += space.ToString() + System.Environment.NewLine ;
                }                    
            }

            return json;
        }

        
    }
}
