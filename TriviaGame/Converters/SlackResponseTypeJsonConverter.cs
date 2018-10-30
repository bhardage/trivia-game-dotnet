using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TriviaGame.Models;

namespace TriviaGame.Converters
{
    public class SlackResponseTypeJsonConverter : JsonConverter<SlackResponseType>
    {
        public override SlackResponseType ReadJson(JsonReader reader, Type objectType, SlackResponseType existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return Enum.Parse<SlackResponseType>((string)reader.Value, true);
        }

        public override void WriteJson(JsonWriter writer, SlackResponseType value, JsonSerializer serializer)
        {
            writer.WriteValue(Enum.GetName(typeof(SlackResponseType), value).ToLower());
        }
    }
}
