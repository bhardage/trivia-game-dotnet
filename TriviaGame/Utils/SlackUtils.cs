using System.Linq;
using System.Text.RegularExpressions;

namespace TriviaGame.Utils
{
    public class SlackUtils
    {
        private const string SLACK_ID_PATTERN = "^<@(.+?)(\\|.*)?>$";

        public static string NormalizeId(string slackId)
        {
            if (slackId == null)
            {
                return null;
            }

            string extractedId = slackId;
            Match match = Regex.Matches(extractedId, SLACK_ID_PATTERN).FirstOrDefault();

            if (match != null)
            {
                extractedId = match.Groups[1].Value;
            }

            return extractedId;
        }
    }
}
