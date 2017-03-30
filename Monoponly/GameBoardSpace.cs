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
        public abstract class BoardSpace
        {
            public string name { get; }
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
                get { return board.FirstOrDefault(s => s.name == name); }
            }

            public BoardSpace this[int spaceNumber]
            {
                get { return board[spaceNumber]; }
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

                foreach (BoardSpace space in board)
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

            public int PropertiesPlayerOwn(Player p, StreetColour StreetColour)
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

            public BoardSpace FindNextUtility(BoardSpace space)
            {
                int index = space.spaceNumber;
                for (int k = 1; k <= board.Count; k++)
                {
                    index = (index + k) % board.Count();
                    if (board[index].GetType() == typeof(Utility))
                        return board[index];
                }
                throw new Exception("Can't find a Utility!");
            }

            public BoardSpace FindNextTrasportation(BoardSpace space)
            {
                int index = space.spaceNumber;
                for (int k = 1; k <= board.Count; k++)
                {
                    index = (index + k) % board.Count();
                    if (board[index].GetType() == typeof(Transportation))
                        return board[index];
                }
                throw new Exception("Can't find a Transportation hub!");
            }

            public int GetHotels(Player player)
            {
                int count = 0;

                var list = board.Where(p => p is Property && ((Property)p).owner == player);

                foreach (Property p in list)
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

            public abstract int RentToPay(Game game, Player player);

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

            public Tax(string Name, int SeqenceNumber, int Tax, int TotalWorthPerc) : base(Name, SeqenceNumber)
            {
                this.taxAmount = Tax;
                this.totalWorthPerc = TotalWorthPerc;
            }
        }

        public class Chance : BoardSpace
        {
            public ChanceType chaneType { get; set; }

            public Chance(string Name, int SeqenceNumber, ChanceType ChanceType) : base(Name, SeqenceNumber)
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

            public Transportation(string Name, int SeqenceNumber, int PurhcasePrice, int MortageValue, int OneRR, int TwoRR, int ThreeRR, int FourRR) : base(Name, SeqenceNumber, PurhcasePrice, MortageValue)
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
            public int oneUtility { get; }
            public int twoUtlility { get; }

            public Utility(string Name, int SeqenceNumber, int PurhcasePrice, int MortageValue, int OneUtility, int TwoUtlility) : base(Name, SeqenceNumber, PurhcasePrice, MortageValue)
            {
                this.oneUtility = OneUtility;
                this.twoUtlility = TwoUtlility;
            }

            public override int RentToPay(Game game, Player player)
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
                if (buildingsOnProperty != 0)
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
    }
}
