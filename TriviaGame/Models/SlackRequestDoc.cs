using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TriviaGame.Models
{
    public class SlackRequestDoc
    {
        public string Token { get; set; }

        [FromForm(Name = "team_id")]
        public string TeamId { get; set; }

        [FromForm(Name = "team_domain")]
        public string TeamDomain { get; set; }

        [FromForm(Name = "enterprise_id")]
        public string EnterpriseId { get; set; }

        [FromForm(Name = "enterprise_name")]
        public string EnterpriseName { get; set; }

        [FromForm(Name = "channel_id")]
        public string ChannelId { get; set; }

        [FromForm(Name = "channel_name")]
        public string ChannelName { get; set; }

        [FromForm(Name = "user_id")]
        public string UserId { get; set; }

        [FromForm(Name = "user_name")]
        public string Username { get; set; }

        public string Command { get; set; }
        public string Text { get; set; }

        [FromForm(Name = "response_url")]
        public string ResponseUrl { get; set; }

        public string TriggerId { get; set; }

        public DateTime RequestTime { get; set; }
    }
}
