using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TriviaGame.Models
{
    public class SlackResponseDoc
    {
        [JsonProperty("response_type")]
        public SlackResponseType ResponseType { get; set; }
        public string Text { get; set; }
        public List<SlackAttachment> Attachments { get; set; }

        public static SlackResponseDoc Failure(string text)
        {
            return new SlackResponseDoc
            {
                ResponseType = SlackResponseType.EPHEMERAL,
                Text = text
            };
        }
    }
}
