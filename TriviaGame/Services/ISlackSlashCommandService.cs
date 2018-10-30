using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TriviaGame.Models;

namespace TriviaGame.Services
{
    public interface ISlackSlashCommandService
    {
        SlackResponseDoc processSlashCommand();
    }
}
