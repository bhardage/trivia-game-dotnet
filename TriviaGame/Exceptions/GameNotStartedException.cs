using System;

namespace TriviaGame.Exceptions
{
    public class GameNotStartedException : Exception
    {
        public GameNotStartedException()
        {
        }

        public GameNotStartedException(string message)
            : base(message)
        {
        }

        public GameNotStartedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
