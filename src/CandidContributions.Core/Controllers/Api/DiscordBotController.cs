using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using CandidContributions.Core.Models.Api.DiscordBot;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.PublishedModels;
using Umbraco.Extensions;

namespace CandidContribs.Core.Controllers.Api
{
    public class DiscordBotController : UmbracoApiController
    {
        private static readonly Lazy<List<TokenRequestFail>> _tokenRequestFails = new Lazy<List<TokenRequestFail>>(() => new List<TokenRequestFail>());
        private static readonly Lazy<string> _token = new Lazy<string>(GenerateToken);
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        public DiscordBotController(IUmbracoContextAccessor umbracoContextAccessor)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        /// <summary>
        /// This endpoint is used to block spamming secrets and supplying a very basic access token
        /// We currently only support 1 client
        /// </summary>
        /// <param name="secret"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Token")]
        public IActionResult Token([FromBody]string secret)
        {
            if (secret.IsNullOrWhiteSpace())
            {
                return BadRequest("secret parameter must be specified");
            }

            var clientIp = GetClientIp();
            if (clientIp == null)
            {
                return BadRequest("Can't detect clientIp");
            }

            if (_tokenRequestFails.Value.Count(r => r.Ip == clientIp && r.DateTime >= DateTime.Now.AddHours(-3)) > 3)
            {
                return BadRequest("To many failed attempts");
            }

            if(_umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext))
            {
                return NotFound("Could not find an Umbraco Context");
            }

            var discordBotFolder = umbracoContext.Content.GetAtRoot()
                .FirstOrDefault(n => n.ContentType.Alias ==DiscordBotFolder.ModelTypeAlias) as DiscordBotFolder;
            if (discordBotFolder?.Secret.IsNullOrWhiteSpace() != false)
            {
                return BadRequest("Invalid server configuration");
            }

            if (secret != discordBotFolder.Secret)
            {
                _tokenRequestFails.Value.Add(new TokenRequestFail(DateTime.Now, clientIp));
                return BadRequest("Invalid secret");
            }

            return Ok(_token.Value);
        }

        [HttpGet]
        [Route("BingoConfiguration")]
        public IActionResult BingoConfiguration(string token, int wordCount)
        {
            if (token != _token.Value)
            {
                return Unauthorized();
            }

            if (_umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext))
            {
                return NotFound("Could not find an Umbraco Context");
            }

            //todo this should be moved into a service
            var discordBotFolder = umbracoContext.Content.GetAtRoot()
                .FirstOrDefault(n => n.ContentType.Alias == DiscordBotFolder.ModelTypeAlias) as DiscordBotFolder;
            var bingoFolder = discordBotFolder.FirstChild<BingoFolder>();
            var configuration = new BingoConfiguration();
            configuration.Words = bingoFolder.FirstChild<BingoWordsFolder>()
                .Children<BingoWord>().Select(w => w.Name).ToList();
            configuration.KeyedPhrases = bingoFolder.FirstChild<BingoPhrasesFolder>().Children<BingoPhrase>()
                .Select(kp => new KeyedPhrases
                { Key = kp.Key, Phrases = kp.Collection.Select(p => new Phrase { Boost = p.Boost, Text = p.Text }).ToList() })
                .ToList();
            var autoRoundSettings = bingoFolder.FirstChild<BingoSettings>().FirstChild<Umbraco.Cms.Web.Common.PublishedModels.AutoRoundSettings>();
            configuration.AutoRoundSettings = new CandidContributions.Core.Models.Api.DiscordBot.AutoRoundSettings
            {
                MaximumTimeout = autoRoundSettings.MaximumTimeout,
                MinimumTimeout = autoRoundSettings.MinimumTimeOut,
                PreferedTimeout = autoRoundSettings.PreferedTimeout,
                PreferedTimeoutSkewPercentage = autoRoundSettings.PreferedTimeoutSkewPercentage
            };

            return Ok(configuration);
        }


        private string GetClientIp(HttpRequest request = null)
        {
            request = request ?? base.Request;

            return request.HttpContext.Connection.RemoteIpAddress.ToString();
        }

        private static string GenerateToken()
        {
            const int length = 250;

            // creating a StringBuilder object()
            var stringBuilder = new StringBuilder();
            var random = new Random();

            for (var i = 0; i < length; i++)
            {
                var flt = random.NextDouble();
                var shift = Convert.ToInt32(Math.Floor(25 * flt));
                var letter = Convert.ToChar(shift + 65);
                stringBuilder.Append(letter);
            }

            return stringBuilder.ToString();
        }
    }
}
