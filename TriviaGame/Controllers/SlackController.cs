﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TriviaGame.Models;
using TriviaGame.Services;

namespace TriviaGame.Controllers
{
    [Route("api/slack/slash")]
    [ApiController]
    public class SlackController : ControllerBase
    {
        private readonly ISlackSlashCommandService _slackSlashCommandService;

        public SlackController(ISlackSlashCommandService slackSlashCommandService)
        {
            _slackSlashCommandService = slackSlashCommandService;
        }

        [HttpPost]
        public SlackResponseDoc Post()
        {
            return _slackSlashCommandService.processSlashCommand();
        }
    }
}