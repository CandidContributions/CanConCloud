using System.IO;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Web.Common.Controllers;

namespace CandidContributions.Core.Controllers.Api
{
    [Route("umbraco/api/GuestbookApi")]
    public class GuestbookApiController : UmbracoApiController
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public GuestbookApiController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet("GetEntries/{id}")]
        public string GetEntries(string id)
        {
            // cheeky hack to keep the 'planting' message visible for a few seconds!!
            Thread.Sleep(3000);

            // guestbook contents must be serialised as json and committed to the repo
            // in v8 this was done by a scheduled process whilst the event was running
            var githubFolder = _webHostEnvironment.ContentRootPath + "\\wwwroot\\";
            var jsonPath = $"{githubFolder}\\github\\{id}.json";

            if (!System.IO.File.Exists(jsonPath))
            {
                return "[]";
            }

            using (var r = new StreamReader(jsonPath))
            {
                var json = r.ReadToEnd();
                return json;
            }
        }
    }
}