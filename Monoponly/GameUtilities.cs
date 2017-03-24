using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monoponly
{
    public class GameUtilities
    {
        public static int RollDice(int Sides)
        {
            Random rand = new Random();
            return rand.Next(1, 6);
        }
    }
}
