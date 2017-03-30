using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monoponly
{
    public partial class Game
    {
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

        public class GoToJailException : Exception
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
    }
}
