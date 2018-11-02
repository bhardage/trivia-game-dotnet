using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TriviaGame.Models;

namespace TriviaGame.Services
{
    public class DelayedSlackService : IDelayedSlackService
    {
        private static readonly HttpClient client = new HttpClient();

        public void sendResponse(string url, SlackResponseDoc responseDoc)
        {
            Task.Run(() => client.PostAsJsonAsync(url, responseDoc));
        }
    }
}
