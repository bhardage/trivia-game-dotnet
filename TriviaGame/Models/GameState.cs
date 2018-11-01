using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TriviaGame.Models
{
    public class GameState
    {
        public string ControllingUserId { get; set; }
        public string Topic { get; set; }
        public string Question { get; set; }
        public List<Answer> Answers { get; set; }

        public class Answer
        {
            public string UserId { get; set; }
            public string Username { get; set; }
            public string Text { get; set; }
            public DateTime CreatedDate { get; set; }
        }
    }
}
