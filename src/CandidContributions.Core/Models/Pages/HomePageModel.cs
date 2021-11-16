using System.Collections.Generic;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace CandidContributions.Core.Models.Pages
{
    public class HomePageModel : Home
    {
        public HomePageModel(IPublishedContent content, IPublishedValueFallback publishedValueFallback)
            : base(content, publishedValueFallback)
        {
        }

        public List<Episode> AllEpisodes { get; set; } = new List<Episode>();
        public Episode LatestEpisode;

        public List<EventsPage> PastEvents { get; set; }
        public List<EventsPage> UpcomingEvents { get; set; }

    }
}