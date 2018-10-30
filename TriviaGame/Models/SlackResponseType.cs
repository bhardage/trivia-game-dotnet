using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TriviaGame.Converters;

namespace TriviaGame.Models
{
    [JsonConverter(typeof(SlackResponseTypeJsonConverter))]
    public enum SlackResponseType
    {
        IN_CHANNEL,
        EPHEMERAL
    }
}
