using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections;

namespace Monoponly
{
    public class Game
    {
        public int houses { get; set; }
        public int hotels { get; set; }
        public DiceSet dice = new DiceSet();
        public BoardSpaceCollection Board = new BoardSpaceCollection();
        public PlayerCollection players = new PlayerCollection();
        public List<RollLogEntry> rollLog = new List<RollLogEntry>();

        public const string Jail = "Jail";

        #region enum

        public enum CCardType
        {
            Chance,
            ComunityChest
        }
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

        #region interfaces
        public interface ICCardMove
        {
            void MovePlayer(Player player);
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
                Console.WriteLine(game.dice.ToString());

                //Move player to jail if this is his third double
                if (IsThirdDouble(game) && !inJail)
                {
                    inJail = true;
                    MovePlayer(game.Board[Jail],false);
                    game.players.NextPlayerTurn();
                    throw new GoToJailException($"{name} has been sent to Jail!");                    
                }

                //If player is in Jail remain in Jail
                if (inJail && ! game.dice.IsMatch())
                {
                    game.players.NextPlayerTurn();
                    throw new RemainInJailException($"{name} must remain in Jail!");
                }

                //Move player out of jail if in Jail. Move to new position and pay salary if needed
                if (game.dice.IsMatch() && inJail)
                {
                    MovePlayer(NewSpaceAfterRoll(game.dice, game),true);
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

            //public void TurnOfNextPlayer(Game game)
            //{
            //    Player p = game.players.Dequeue();
            //    game.players.Enqueue(p);

            //}
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

        public class BoardSpaceCollection : IEnumerable
        {
            List<BoardSpace> board;

            public int Count => board.Count;

            public BoardSpaceCollection()
            {
                board = new List<BoardSpace>();
            }

            public BoardSpace this[string name]
            {
                get{ return board.FirstOrDefault(s => s.name == name);}
            }

            public BoardSpace this[int spaceNumber]
            {
                get { return board[spaceNumber];}
            }

            public void Add(BoardSpace space)
            {
                if (board.Count > 0)
                {
                    BoardSpace exisitng = board.FirstOrDefault<BoardSpace>(b => b.name == space.name);
                    if (exisitng != null)
                        throw new Exception("There is an exisiting property with this name!");
                }

                board.Add(space);
            }            

            public int UtilitiesPlayerOwn(Player p)
            {
                int own = 0;

                foreach(BoardSpace space in board)
                {
                    if (space is Utility && (((Utility)space).owner == p))
                        own++;
                }

                return own;
            }

            public int TransportionPlayerOwn(Player p)
            {
                int own = 0;

                foreach (BoardSpace space in board)
                {
                    if (space is Transportation && (((Transportation)space).owner == p))
                        own++;
                }

                return own;
            }

            public int PropertiesPlayerOwn(Player p,StreetColour StreetColour)
            {
                int own = 0;

                foreach (BoardSpace space in board)
                {
                    if (
                        space is Property 
                        && (((Property)space).owner == p) 
                        && (((Property)space).streetColour == StreetColour)
                        )
                    { own++; }                      
                }

                return own;
            }

            public int GetHotels(Player player)
            {
                int count = 0;

                var list = board.Where(p => p is Property && ((Property)p).owner == player );
                
                foreach(Property p in list)
                {
                    if (p.buildingsOnProperty == 5) count++;
                }

                return count;
            }

            public int GetHouses(Player player)
            {
                int count = 0;

                var list = board.Where(p => p is Property && ((Property)p).owner == player);

                foreach (Property p in list)
                {
                    if ((p.buildingsOnProperty > 0) && (p.buildingsOnProperty < 5))
                            count += p.buildingsOnProperty;
                }

                return count;
            }


            IEnumerator IEnumerable.GetEnumerator()
            {
                return board.GetEnumerator();
            }
        }

        public abstract class PurchasableBoardSpace : BoardSpace
        {
            public int purchasePrice { get; }
            public int mortageValue { get; }
            public bool isMortaged { get; set; }
            public int[] rent { get; }
            public Player owner { get; set; }            

            public abstract int RentToPay(Game game,Player player);                      

            public PurchasableBoardSpace(string Name, int SeqenceNumber, int PurhcasePrice, int MortageValue) : base(Name, SeqenceNumber)
            {
                this.purchasePrice = PurhcasePrice;
                this.mortageValue = MortageValue;
                this.isMortaged = false;
            }

            public bool IsOwner(Player player)
            {
                return (owner == player) ? true : false;
            }
        }

        public class Corner : BoardSpace
        {
            public CornerType cornerType { get; }

            public Corner(string Name, int SeqenceNumber, CornerType CornerType) : base(Name, SeqenceNumber)
            {
                this.cornerType = CornerType;
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

            public override int RentToPay(Game game, Player player)
            {
                if (owner == player || owner == null || isMortaged)
                    return 0;
                else
                {
                    switch (game.Board.TransportionPlayerOwn(player))
                    {
                        case 1:
                            return oneRR;                            
                        case 2:
                            return twoRR;                            
                        case 3:
                            return threeRR;                            
                        case 4:
                            return fourRR;                            
                        default:
                            throw new Exception($"Invalid amount({game.Board.TransportionPlayerOwn(player)}) of railroads owned!");
                    }
                }                                    
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

            public override int RentToPay(Game game,Player player)
            {
                if (owner == player || owner == null || isMortaged)
                    return 0;
                else
                {
                    switch (game.Board.UtilitiesPlayerOwn(player))
                    {
                        case 1:
                            return oneUtility * game.dice.RollTotal();
                        case 2:
                            return twoUtlility * game.dice.RollTotal();                     
                        default:
                            throw new Exception($"Invalid amount({game.Board.TransportionPlayerOwn(player)}) of Utilities owned!");
                    }
                }
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

            public override int RentToPay(Game game, Player player)
            {
                int own = 0;

                //No rent to pay
                if (owner == player || owner == null || isMortaged)
                    return 0;

                own = game.Board.PropertiesPlayerOwn(player, streetColour);                

                //Dont own all streets in colours 
                if (buildingsOnProperty == 0 && own != streetInColour)
                    return rent;

                //Owns all streets in colour, no buildings
                if (buildingsOnProperty == 0 && own != streetInColour)
                    return 2 * rent;

                //Owns buildings
                if(buildingsOnProperty != 0)                
                {
                    switch (buildingsOnProperty)
                    {
                        case 1:
                            return house1;
                        case 2:
                            return house2;
                        case 3:
                            return house4;
                        case 4:
                            return house4;
                        case 5:
                            return hotel;
                        default:
                            throw new Exception($"Invalid number of buildings({own}) on property!");
                    }
                }

                throw new Exception($"Failure calculating rent for {name}");
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

            public RollLogEntry(Player player,DiceSet dice)
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

        public abstract class CCard
        {
            public CCardType ccardType { get; }
            public String description { get; }                 
            
            public CCard(CCardType CCardType,string Description)
            {
                this.ccardType = CCardType;
                this.description = Description;
            }

        }

        public class CCardPayment : CCard
        {
            //Give negative values to deduct money
            public int amount { get; }

             public CCardPayment(CCardType CCardType, string Description,int Amount) : base(CCardType, Description)
            {                
                this.amount = Amount;
            }
            
            public void ProcessPayment(Player player)
            {
                //Negative amounts for payments
                if (amount < 0) 
                    player.DeductMoney(Math.Abs(amount));
                else
                    player.money += amount;
            }            
        }

        public class CCardPaymentAllPlayer : CCardPayment
        {
            public bool includeCurrentPLayer { get; }

            public CCardPaymentAllPlayer(CCardType CCardType, string Description, int Amount,bool IncludeCurrentPlayer) : base(CCardType, Description, Amount)
            {
                includeCurrentPLayer = IncludeCurrentPlayer;
            }

            public void ProcessPlayersPayment(Game game)
            {
                int total = 0;

                foreach (Player p in game.players)
                {
                    ProcessPayment(p);
                    if (amount > 0)
                        total += amount;
                    else
                        total -= amount;
                    //?????/?
                }
            }
        }

        public class CCardMoveTo : CCard
        {
            BoardSpace space { get; }

            public CCardMoveTo(CCardType CCardType, string Description, BoardSpace Property) : base(CCardType, Description)
            {
                this.space = Property;
            }

            public void PerformAction(Game game,Player player)
            {
                if (space is Property)
                {
                    player.MovePlayer(space, true);
                    //player.money -= ((Property)space).RentToPay(game, player);
                    return;
                }

                if(space is Utility)
                {
                    player.MovePlayer(space, true);
                    //player.money -= ((Utility)space).RentToPay(game, player) * 2;
                    return;
                }

                if(space is Transportation)
                {
                    player.MovePlayer(space, true);
                    //player.money -= ((Transportation)space).RentToPay(game, player);
                    return;
                }

                if(space is Corner)
                {
                    switch(((Corner)space).cornerType)
                    {
                        case CornerType.FreeParking:
                            player.MovePlayer(space, true);
                            return;

                        case CornerType.Jail:
                            player.MovePlayer(space, false);
                            player.inJail = true;
                            game.players.NextPlayerTurn();
                            return;

                        case CornerType.Start:
                            player.MovePlayer(space, false);                            
                            game.players.NextPlayerTurn();
                            return;
                    }

                }
            }            
            
        }

        public class CCardGetOutOfJail : CCard
        {
            public Player owner { get; set; }

            public CCardGetOutOfJail(CCardType CCardType, string Description) : base(CCardType, Description)
            { }                                    
            
        }

        public class CCardStreetRepairs : CCard
        {
            public int costPerHouse { get; }
            public int costPerHotel { get; }

            public CCardStreetRepairs(CCardType CCardType, string Description, int House, int Hotel) : base(CCardType, Description)
            {
                costPerHouse = House;
                costPerHotel = Hotel; 
            }


            public void PayForRepairs(Game game,Player player)
            {
                player.DeductMoney((costPerHotel * game.Board.GetHotels(player)) + (costPerHouse * game.Board.GetHotels(player)));
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

        public class PlayerAlreadyExistsException : Exception
        {
            public PlayerAlreadyExistsException()
            {
            }

            public PlayerAlreadyExistsException(string message) : base(message)
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
            players.Add(new Player("Player One", PlayerToker.Boot,Board["Go"]));
            players.Add(new Player("Player Two", PlayerToker.Car, Board["Go"]));
            players.Add(new Player("Player Three", PlayerToker.Cat, Board["Go"]));
            players.Add(new Player("Player Four", PlayerToker.Dog, Board["Go"]));

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
