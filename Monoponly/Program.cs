using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monoponly
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //Game game = new Game();
                //Console.WriteLine(game.ToString());            

                LinkedList<int> list = new LinkedList<int>();

                list.AddFirst(10);
                list.AddLast(20);
                list.AddLast(30);
                list.AddLast(40);
                list.AddLast(50);
                list.AddLast(60);

                Console.WriteLine(list.First.Value.ToString());
                                               

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
