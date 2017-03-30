using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monoponly
{
    partial class Game
    {
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

        public interface ICCard
        {
            void PerformActions(Game game, Player player);
        }



    }
}
