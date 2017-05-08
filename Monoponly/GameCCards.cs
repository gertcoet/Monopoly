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
            public string description { get; }

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
            public bool receiveMoney { get; }

            public CCardPaymentAllPlayer(CCardType CCardType, string Description, int Amount, bool ReceiveMoney) : base(CCardType, Description, Amount)
            {
                receiveMoney = ReceiveMoney;
            }

            public override void PerformActions(Game game, Player player)
            {
                int absAmount = Math.Abs(amount);

                if (this.receiveMoney)//Other payer pay you
                {
                    foreach (Player p in game.players)
                    {
                        if (player != p)
                        {
                            p.DeductMoney(absAmount);
                            player.money += absAmount;
                        }
                    }
                }
                else
                {
                    foreach (Player p in game.players)//Pay other players                    
                    {
                        if (player != p)
                        {
                            p.money += absAmount;
                            player.DeductMoney(absAmount);
                        }

                    }
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
                            game.SendPlayerToJail(player, description);
                            //game.players.NextPlayerTurn();
                            return;

                        case CornerType.Start:
                            player.MovePlayer(space, true);
                            //game.players.NextPlayerTurn();
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
                player.DeductMoney((Math.Abs(costPerHotel) * game.Board.GetHotels(player)) + (Math.Abs(costPerHouse) * game.Board.GetHotels(player)));
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
