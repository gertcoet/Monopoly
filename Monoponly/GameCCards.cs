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
        public abstract class CCard : ICCard
        {
            public CCardType ccardType { get; }
            public String description { get; }

            public CCard(CCardType CCardType, string Description)
            {
                this.ccardType = CCardType;
                this.description = Description;
            }

            public abstract void PerformActions(Game game, Player player);
            
        }
        public class CCardPayment : CCard 
        {
            //Give negative values to deduct money
            public int amount { get; }

            public CCardPayment(CCardType CCardType, string Description, int Amount) : base(CCardType, Description)
            {
                this.amount = Amount;
            }

            public override void PerformActions(Game game,Player player)
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

            public CCardPaymentAllPlayer(CCardType CCardType, string Description, int Amount, bool IncludeCurrentPlayer) : base(CCardType, Description, Amount)
            {
                includeCurrentPLayer = IncludeCurrentPlayer;
            }

            public override void PerformActions(Game game, Player player)
            {
                int total = 0;

                foreach (Player p in game.players)
                {
                    if (player != p)
                    {
                        if (amount < 0)
                            p.money += Math.Abs(amount); //Pay other players                    
                        else
                            p.DeductMoney(Math.Abs(amount)); //Other payer pay you

                        total += amount;
                    }
                }

                if (amount < 0)
                    player.DeductMoney(total); //Deduct total from player
                else
                    player.money += total; //Add total to player
            }
        }

        public class CCardMoveTo : CCard
        {
            BoardSpace space { get; }

            public CCardMoveTo(CCardType CCardType, string Description, BoardSpace Property) : base(CCardType, Description)
            {
                this.space = Property;
            }

            public override void PerformActions(Game game, Player player)
            {
                if (space is Property)
                {
                    player.MovePlayer(space, true);
                    //player.money -= ((Property)space).RentToPay(game, player);
                    return;
                }

                if (space is Utility)
                {                   
                    player.MovePlayer(game.Board.FindNextUtility(game.Board[player.currPos.spaceNumber]), true);
                    //player.money -= ((Utility)space).RentToPay(game, player) * 2;
                    return;
                }

                if (space is Transportation)
                {
                    player.MovePlayer(space, true);
                    //player.money -= ((Transportation)space).RentToPay(game, player);
                    return;
                }

                if (space is Corner)
                {
                    switch (((Corner)space).cornerType)
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
                            player.MovePlayer(space, true);
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

            public override void PerformActions(Game game, Player player)
            {
                owner = player;
            }
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

            public override void PerformActions(Game game, Player player)
            {
                player.DeductMoney((costPerHotel * game.Board.GetHotels(player)) + (costPerHouse * game.Board.GetHotels(player)));
            }            

        }

        public class CCardMoveSpaces : CCard
        {
            public int spacesToMove { get; }
            public CCardMoveSpaces(CCardType CCardType, string Description, int SpacesToMove) : base(CCardType, Description)
            {
                spacesToMove = SpacesToMove;
            }

            public override void PerformActions(Game game, Player player)
            {
                player.MovePlayer(spacesToMove, true, game);
            }


        }

        public class CCardCollection : IEnumerable
        {
            private Queue<CCard> cards;          
            public CCardCollection(List<CCard> Cards)
            {
                Cards.Shuffle<CCard>();
                cards = new Queue<CCard>();

                foreach (CCard card in Cards)
                {
                    cards.Enqueue(card);
                }                                
            }

            public CCard GetCard()
            {
                CCard card = cards.Dequeue();
                cards.Enqueue(card);
                return card;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return cards.GetEnumerator();
            }

        }
    }
}
