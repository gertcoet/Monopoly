using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Monoponly
{
    internal class Game
    {
        internal int houses { get; set; }
        internal int hotels { get; set; }
        internal DiceSet dice = new DiceSet();
        internal Dictionary<int,BoardSpace> Board = new Dictionary<int,BoardSpace>();
        internal List<Player> players = new List<Player>();

        #region enum
        internal enum PlayerToker
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

        internal enum CornerType
        {
            Start,
            Jail,
            FreeParking,
            GoToJail
        }

        internal enum StreetColour
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

        internal enum ChanceType
        {
            Chance,
            CommunityChest
        }
        #endregion

        #region InternalClasses
        internal class Player
        {
            internal string name { get; set; }
            internal int money { get; set; }
            internal bool inJail { get; set; }
            internal bool isSolvant { get; set; }
            internal PlayerToker playerToken { get; set; }
            internal List<DiceSet> rollLog { get; set; }
            internal BoardSpace currPos { get; set; }
            internal BoardSpace prevPos { get; set; }

            static private int salary;

            internal Player(string Name,PlayerToker PlayerToken,BoardSpace StartingPoint)
            {
                name = Name;
                money = 1500;
                inJail = false;
                isSolvant = true;
                playerToken = PlayerToken;
                currPos = StartingPoint;
            }
            private void LogRollValues(DiceSet dice)
            {
                rollLog.Add(dice);
            }
            internal bool IsThirdDouble()
            {
                for(int k = rollLog.Count;k >= rollLog.Count - 3; k--)
                {
                    if (!(rollLog[k].IsMatch()))
                        return false;                    
                }

                return true;
            }
            internal int RollDice(Game game)
            {
                DiceSet dice = new DiceSet();
                dice.Roll();
                rollLog.Add(dice);

                //Move player to jail                
                if (IsThirdDouble() && !inJail)
                {
                    inJail = true;
                    prevPos = currPos;
                    currPos = game.GetJail();

                    throw new GoToJailException($"{name} has been sent to Jail!");
                }
                else
                //Move player out of jail                
                if (dice.IsMatch() && inJail)
                {
                    inJail = false;                    
                    //throw new GoToJailException($"{name} has been sent to Jail!");
                }

                return dice.RollTotal();
            }
            internal int DeductMoney(int Amount)
            {
                if (Amount > money)
                    throw new InsufficientFundsException($"{name} doesn not have enough money to complete this transaction ({money}) ");

                return money=-Amount;
            }
            internal void GiveSalary()
            {
                money += salary;
            }
            internal void movePlayer(int Positions,Game game)
            {               
            }
                                      
        }

        internal abstract class BoardSpace
        {
            internal string name { get;  }
            internal int spaceNumber { get; }            

            protected BoardSpace(String Name, int SpaceNumber)
            {
                this.name = Name;                
                this.spaceNumber = SpaceNumber;
            }
        }

        internal abstract class PurchasableBoardSpace : BoardSpace
        {
            internal int purchasePrice { get; }
            internal int mortageValue { get; }
            internal bool isMortaged { get; set; }
            internal int[] rent { get; }
            internal Player owner { get; set; }

            internal PurchasableBoardSpace(string Name, int SeqenceNumber, int PurhcasePrice, int MortageValue) : base(Name, SeqenceNumber)
            {
                this.purchasePrice = PurhcasePrice;
                this.mortageValue = MortageValue;
                this.isMortaged = false;
            }
        }

        internal class Corner : BoardSpace
        {
            internal CornerType corner { get; }
            internal Corner(string Name, int SeqenceNumber, CornerType Corner) : base(Name, SeqenceNumber)
            {
                this.corner = corner;
            }
        }


        protected class Tax : BoardSpace
        {
            internal int taxAmount { get; }
            internal int totalWorthPerc { get; }

            internal Tax(string Name, int SeqenceNumber, int Tax, int TotalWorthPerc ) : base(Name, SeqenceNumber)
            {
                this.taxAmount = Tax;
                this.totalWorthPerc = TotalWorthPerc;
            }

        }

        protected class Chance : BoardSpace
        {
            internal ChanceType chaneType { get; set; }

            internal Chance(string Name, int SeqenceNumber , ChanceType ChanceType) : base(Name, SeqenceNumber)
            {
                this.chaneType = ChanceType;
            }
        }

        protected class Transportation : PurchasableBoardSpace
        {
            internal int oneRR { get; set; }
            internal int twoRR { get; set; }
            internal int threeRR { get; set; }
            internal int fourRR { get; set; }

            public Transportation(string Name, int SeqenceNumber, int PurhcasePrice, int MortageValue , int OneRR, int TwoRR, int ThreeRR, int FourRR) : base(Name, SeqenceNumber, PurhcasePrice, MortageValue)
            {
                this.oneRR = OneRR;
                this.twoRR = TwoRR;
                this.threeRR = ThreeRR;
                this.fourRR = FourRR;
            }
        }

        protected class Utility : PurchasableBoardSpace
        {
            internal int oneUtility { get;  }
            internal int twoUtlility { get;  }
            internal Utility(string Name, int SeqenceNumber, int PurhcasePrice, int MortageValue, int OneUtility, int TwoUtlility) : base(Name, SeqenceNumber, PurhcasePrice, MortageValue)
            {
                this.oneUtility = OneUtility;
                this.twoUtlility = TwoUtlility;
            }
        }

        protected class Property : PurchasableBoardSpace
        {
            internal int streetInColour { get; set; }
            internal int buildingCost { get; set; }
            internal StreetColour streetColour { get; set; }
            internal int buildingsOnProperty { get; set; }

            internal int rent { get; }
            internal int house1 { get; }
            internal int house2 { get; }
            internal int house3 { get; }
            internal int house4 { get; }
            internal int hotel { get; }

            internal Property(string Name, int SeqenceNumber, int PurhcasePrice, int MortageValue, int Rent, int House1, int House2, int House3, int House4, int Hotel) : base(Name, SeqenceNumber, PurhcasePrice, MortageValue)
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

        protected class DiceSet
        {
            internal int dice1 { get; set; }
            internal int dice2 { get; set; }

            internal void Roll()
            {
                dice1 = GameUtilities.RollDice(6);
                dice2 = GameUtilities.RollDice(6);
            }

            internal Boolean IsMatch()
            {
                return dice1 == dice2;                    
            }

            internal int RollTotal()
            {
                return dice1 + dice2;
            }
        }
        #endregion

        #region Exceptions
        internal class InsufficientFundsException : Exception
        {
            internal InsufficientFundsException()
            {
            }

            internal InsufficientFundsException(string message) : base(message)
            {
            }
        }

        internal class GoToJailException: Exception
        {
            internal GoToJailException()
            {
            }

            internal GoToJailException(string message) : base(message)
            {
            }
        }

        #endregion
        public override string ToString()
        {
            string str = string.Empty;
            foreach (BoardSpace space in Board)
            {
                str = str + $"{{{space.seqenceNumber.ToString()};{space.name}}}";
            }

            return str;
        }

        public Game()
        {
            // First block
            Board.Add(0,new Corner("Go", 0, CornerType.Start));
            Board.Add(1,new Property("Old Kent Road", 1, 60, 2, 30, 10, 30, 90, 160, 250) { streetInColour = 2, streetColour = StreetColour.Brown });
            Board.Add(2,new Chance("Community Chest", 2, ChanceType.CommunityChest));
            Board.Add(3,new Property("Whitechapel Road", 3, 60, 30, 4, 20, 60, 180, 320, 450) { streetInColour = 2, streetColour = StreetColour.Brown });
            Board.Add(4,new Tax("Income Tax", 4, 200, 10));
            Board.Add(5,new Transportation("Kings Cross Station", 5, 200, 100, 25, 50, 100, 200));
            Board.Add(6,new Property("The Angel Islington", 6, 100, 50, 6, 30, 90, 270, 400, 550) { streetInColour = 3, streetColour = StreetColour.LightBlue });
            Board.Add(7,new Chance("Chance", 7, ChanceType.Chance));
            Board.Add(8,new Property("Euston Road", 8, 100, 50, 6, 30, 90, 270, 400, 550) { streetInColour = 3, streetColour = StreetColour.LightBlue });
            Board.Add(9,new Property("Pentonville Road", 9, 120, 60, 8, 40, 100, 300, 450, 600) { streetInColour = 3, streetColour = StreetColour.LightBlue });
            // second block
            Board.Add(10,new Corner("Jail", 10, CornerType.Jail));
            Board.Add(11,new Property("Pall Mall", 11, 140, 70, 10, 50, 150, 450, 625, 750) { streetInColour = 3, streetColour = StreetColour.Pink });
            Board.Add(12,new Utility("Power Utility", 12, 150, 75, 4, 10));
            Board.Add(13,new Property("White Hall", 13, 140, 70, 10, 50, 150, 450, 625, 750) { streetInColour = 3, streetColour = StreetColour.Pink });
            Board.Add(14,new Property("Northumberland Avenue", 14, 160, 80, 12, 60, 180, 500, 700, 900) { streetInColour = 3, streetColour = StreetColour.Pink });
            Board.Add(15,new Transportation("Marylebone Station", 15, 200, 100, 25, 50, 100, 200));
            Board.Add(16,new Property("Bow Street", 16, 180, 90, 14, 70, 200, 550, 750, 950) { streetInColour = 3, streetColour = StreetColour.Organe, buildingCost = 100 });
            Board.Add(17,new Chance("Community Chest", 17, ChanceType.Chance));
            Board.Add(18,new Property("Marlborough Street", 18, 180, 90, 14, 70, 200, 550, 750, 950) { streetInColour = 3, streetColour = StreetColour.Organe, buildingCost = 100 });
            Board.Add(19,new Property("Vine Street", 19, 200, 100, 16, 80, 220, 600, 800, 1000) { streetInColour = 3, streetColour = StreetColour.Organe, buildingCost = 100 });
            // Third block
            Board.Add(20,new Corner("Jail", 20, CornerType.FreeParking));
            Board.Add(21,new Property("The Strand", 21, 220, 110, 18, 90, 250, 700, 875, 1050) { streetInColour = 3, streetColour = StreetColour.Red, buildingCost = 150 });
            Board.Add(22,new Chance("Chance", 22, ChanceType.Chance));
            Board.Add(23,new Property("Fleet Street", 23, 220, 110, 18, 90, 250, 700, 875, 1050) { streetInColour = 3, streetColour = StreetColour.Red, buildingCost = 150 });
            Board.Add(24,new Property("Trafalgar Square", 24, 240, 120, 20, 100, 300, 750, 925, 1100) { streetInColour = 3, streetColour = StreetColour.Red, buildingCost = 150 });
            Board.Add(25,new Transportation("Fenchurch St Station", 25, 200, 100, 25, 50, 100, 200));
            Board.Add(26,new Property("Leicester Square", 26, 260, 130, 22, 110, 330, 800, 975, 1150) { streetInColour = 3, streetColour = StreetColour.Yellow, buildingCost = 150 });
            Board.Add(27,new Property("Coventry Street", 27, 260, 130, 22, 110, 330, 800, 975, 1150) { streetInColour = 3, streetColour = StreetColour.Yellow, buildingCost = 150 });
            Board.Add(28,new Utility("Water Utility", 28, 150, 75, 4, 10));
            Board.Add(29,ew Property("Piccadilly", 29, 280, 140, 22, 120, 360, 850, 1025, 1200) { streetInColour = 3, streetColour = StreetColour.Yellow, buildingCost = 150 });
            // Fourth block
            Board.Add(30,new Corner("Jail", 30, CornerType.GoToJail));
            Board.Add(31,new Property("Regent Street", 31, 300, 150, 26, 130, 390, 900, 1100, 1275) { streetInColour = 3, streetColour = StreetColour.Green, buildingCost = 200 });
            Board.Add(32,new Property("Oxford Street", 32, 300, 150, 26, 130, 390, 900, 1100, 1275) { streetInColour = 3, streetColour = StreetColour.Green, buildingCost = 200 });
            Board.Add(33,new Chance("Community Chest", 33, ChanceType.CommunityChest));
            Board.Add(34,new Property("Bond Street", 34, 320, 160, 28, 150, 450, 1000, 1200, 1400) { streetInColour = 3, streetColour = StreetColour.Green, buildingCost = 200 });
            Board.Add(35,new Transportation("Liverpool Street Station", 35, 200, 100, 25, 50, 100, 200));
            Board.Add(36,new Chance("Chance", 36, ChanceType.Chance));
            Board.Add(37,new Property("Park Lane", 37, 350, 175, 35, 175, 500, 1100, 1300, 1500) { streetInColour = 2, streetColour = StreetColour.Blue, buildingCost = 200 });
            Board.Add(38,new Tax("Luxury Tax", 38, 100, 0));
            Board.Add(39,new Property("Mayfair", 39, 400, 200, 50, 200, 600, 1400, 1700, 2000) { streetInColour = 2, streetColour = StreetColour.Blue, buildingCost = 200 });

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

        internal BoardSpace GetJail()
        {
            foreach (KeyValuePair<int,BoardSpace> space in Board)
            {
                if (space.Value.name == "Jail")
                    return space.Value;
            }

            return null;
        }

        public bool MovePlayer(Player player)
        {
            int newPos = ((player.currPos.spaceNumber + player.RollDice(this)) % Board.Count);
            

        }

        
    }
}
