using System;

namespace TriviaGame.Exceptions
{
    public class ScoreException : Exception
    {
        public ScoreException()
        {
        }

        public ScoreException(string message)
            : base(message)
        {
        }

        public ScoreException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
