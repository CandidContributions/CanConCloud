using System;
using System.Collections.Generic;
using System.Linq;
using CandidContributions.Core.Models.Pages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.PublishedModels;
using Umbraco.Extensions;

namespace CandidContributions.Core.Controllers
{
   public class HomeController : RenderController
    {
        private readonly IVariationContextAccessor _variationContextAccessor;
        private readonly ServiceContext _serviceContext;

        public HomeController(ILogger<HomeController> logger, ICompositeViewEngine compositeViewEngine,
            IUmbracoContextAccessor umbracoContextAccessor,
            IVariationContextAccessor variationContextAccessor,
            ServiceContext context)
            : base(logger, compositeViewEngine, umbracoContextAccessor)
        {
            _variationContextAccessor = variationContextAccessor;
            _serviceContext = context;
        }

        public override IActionResult Index()
        {
            var homePageModel = new HomePageModel(CurrentPage, new PublishedValueFallback(_serviceContext, _variationContextAccessor))
            {
                PastEvents = new List<EventsPage>(),
                UpcomingEvents = new List<EventsPage>()
            };

            var episodesFolder = CurrentPage.Children.FirstOrDefault(x => x.ContentType.Alias == EpisodesFolder.ModelTypeAlias);
            if (episodesFolder != null)
            {
                var episodes = episodesFolder.Children;
                foreach (var ep in episodes)
                {
                    homePageModel.AllEpisodes.Add((Episode) ep);
                }
                homePageModel.LatestEpisode = Enumerable.OrderByDescending<Episode, DateTime>(homePageModel.AllEpisodes, x => x.PublishedDate).FirstOrDefault();
            }

            var allEventPages = CurrentPage.Children.Where(x => x.ContentType.Alias == EventsPage.ModelTypeAlias);
            foreach (var page in allEventPages)
            {
                if (!page.IsVisible()) continue;
                if (!(page is EventsPage eventsPage)) continue;

                if (eventsPage.Part2StartDate >= DateTime.Today)
                {
                    homePageModel.UpcomingEvents.Add(eventsPage);
                }
                else
                {
                    homePageModel.PastEvents.Add(eventsPage);
                }
            }

            homePageModel.PastEvents = homePageModel.PastEvents.OrderByDescending(x => x.Part1StartDate).ToList();
            return CurrentTemplate(homePageModel);
        }
    }
}