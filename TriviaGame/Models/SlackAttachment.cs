using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TriviaGame.Models
{
    public class SlackAttachment
    {
        public string Text { get; }
        [JsonProperty("mrkdwn_in")]
        public List<string> MarkdownIn { get; }

        public SlackAttachment(string text)
        {
            this.Text = text;
            this.MarkdownIn = new List<string> { "text" };
        }

        public SlackAttachment(string text, bool allowMarkdown)
        {
            this.Text = text;
            this.MarkdownIn = allowMarkdown ? new List<string> { "text" } : new List<string>();
        }
    }
}
