using System;
using System.Linq;
using System.Net.Http;
using CandidContributions.Core.Models.Api;
using CandidContributions.Core.Models.Configuration;
using Hangfire;
using Hangfire.Console;
using Hangfire.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Common.PublishedModels;
using Umbraco.Extensions;

namespace CandidContributions.Core.Composers
{
    public class SpreakerComposer : ComponentComposer<SpreakerComponent>
    {
        public override void Compose(IUmbracoBuilder builder)
        {
            base.Compose(builder);
            builder.Services.Configure<SpreakerApiOptions>(builder.Config.GetSection("SpreakerApi"));
        }
    }

    public class SpreakerComponent : IComponent
    {
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly SpreakerApiOptions _spreakerOptions;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IContentService _contentService;

        public SpreakerComponent(IOptions<SpreakerApiOptions> spreakerOptions, IHttpClientFactory clientFactory, IContentService contentService, IUmbracoContextFactory umbracoContextFactory)
        {
            _clientFactory = clientFactory;
            _contentService = contentService;
            _umbracoContextFactory = umbracoContextFactory;
            _spreakerOptions = spreakerOptions.Value;
        }

        public void Initialize()
        {
            RecurringJob.AddOrUpdate(() => RetrieveEpisodes(null, _spreakerOptions), Cron.Hourly());
        }

        public void Terminate()
        {
        }

        public void RetrieveEpisodes(PerformContext context, SpreakerApiOptions options)
        {
            const string spreakerApiEpisodesUrlFormat = "https://api.spreaker.com/v2/shows/{0}/episodes";
            const string spreakerApiEpisodeUrlFormat = "https://api.spreaker.com/v2/episodes/{0}";

            if (!options.Enabled)
            {
                context.WriteLine("Spreaker Api disabled");
                return;
            }

            using var cref = _umbracoContextFactory.EnsureUmbracoContext();

            // get episodes folder to add episodes to
            var cache = cref.UmbracoContext.Content;
            var cmsEpisodesFolder = (EpisodesFolder) cache.GetByXPath("//episodesFolder").FirstOrDefault();
            if (cmsEpisodesFolder == null)
            {
                context.WriteLine("Spreaker episode import failed: no EpisodesFolder found");
                return;
            }

            var episodesApiUrl = string.Format(spreakerApiEpisodesUrlFormat, options.ShowId);
            var request = new HttpRequestMessage(HttpMethod.Get, episodesApiUrl);

            var client = _clientFactory.CreateClient();
            var response = client.SendAsync(request).Result;

            if (!response.IsSuccessStatusCode)
            {
                context.WriteLine("Spreaker episode import failed: response code {0}, url {1}", response.StatusCode,
                    episodesApiUrl);
                return;
            }

            // get API response for all episodes
            var episodesString = response.Content.ReadAsStringAsync().Result;
            var convertedEps = JsonConvert.DeserializeObject<APIResponse>(episodesString);

            // get episodes in ascending date order before trying to add to CMS
            var episodes = convertedEps.Response.Items.OrderBy(x => x.PublishedDate);

            var progressBar = context.WriteProgressBar();

            foreach (var episode in episodes.WithProgress(progressBar, episodes.Count()))
            {

                // is this the best way to find by API id?
                var cmsEpisode = cmsEpisodesFolder.SearchChildren(episode.Id.ToString()).FirstOrDefault();
                if (cmsEpisode != null)
                {
                    // already exists so nothing to do
                    context.WriteLine($"Episode: {cmsEpisode.Content.Name} already exists");
                    continue;
                }

                var episodeDetailsUrl = string.Format(spreakerApiEpisodeUrlFormat, episode.Id);
                var episodeDetailsResponse = client.GetAsync(episodeDetailsUrl).Result;

                if (!episodeDetailsResponse.IsSuccessStatusCode)
                {
                    context.WriteLine("Spreaker episode import failed: response code {0}, url {1}",
                        response.StatusCode, episodesApiUrl);
                    continue;
                }

                var episodeString = episodeDetailsResponse.Content.ReadAsStringAsync().Result;
                var convertedEp = JsonConvert.DeserializeObject<APIResponse>(episodeString);

                context.WriteLine($"Adding new episode: {episode.Id} {episode.Title}");
                AddNewEpisode(convertedEp.Response.Episode, cmsEpisodesFolder);
            }
        }

        private void AddNewEpisode(EpisodeModel spreakerEpisode, EpisodesFolder cmsEpisodesFolder)
        {
            var cmsEpisode = _contentService.Create(spreakerEpisode.Title, cmsEpisodesFolder.Id, Episode.ModelTypeAlias, -1);
            cmsEpisode.SetValue("spreakerId", spreakerEpisode.Id);
            cmsEpisode.SetValue("podcastTitle", spreakerEpisode.Title);
            cmsEpisode.SetValue("podcastLink", spreakerEpisode.PlaybackUrl);
            cmsEpisode.SetValue("showNotes", spreakerEpisode.Description);
            cmsEpisode.SetValue("publishedDate", spreakerEpisode.PublishedDate);
            cmsEpisode.SetValue("listensCount", spreakerEpisode.GetListens());

            // try and set the correct title for displaying on web page
            // format should be either Episode X: title, or SX EpY: Episode
            if (spreakerEpisode.Title.Contains(":"))
            {
                cmsEpisode.SetValue("displayTitle", spreakerEpisode.Title.Split(':')[1].Trim());
            }

            _contentService.SaveAndPublish(cmsEpisode);
        }
    }
}
