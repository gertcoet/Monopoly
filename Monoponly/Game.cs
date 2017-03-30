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
        public int houses { get; set; }
        public int hotels { get; set; }
        public DiceSet dice = new DiceSet();
        public BoardSpaceCollection Board = new BoardSpaceCollection();
        public PlayerCollection players = new PlayerCollection();
        public List<RollLogEntry> rollLog = new List<RollLogEntry>();

        public const string Jail = "Jail";

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
            players.Add(new Player("Player One", PlayerToker.Boot, Board["Go"]));
            players.Add(new Player("Player Two", PlayerToker.Car, Board["Go"]));
            players.Add(new Player("Player Three", PlayerToker.Cat, Board["Go"]));
            players.Add(new Player("Player Four", PlayerToker.Dog, Board["Go"]));

        }

        public override string ToString()
        {           
            return JsonConvert.SerializeObject(this);            
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
