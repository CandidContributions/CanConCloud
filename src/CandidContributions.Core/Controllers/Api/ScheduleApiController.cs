using System;
using System.Collections.Generic;
using System.Linq;
using CandidContributions.Core.Models.Api;
using CandidContributions.Core.Models.Shared;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.PublishedModels;
using Umbraco.Extensions;
using DayScheduleEntry = CandidContributions.Core.Models.Api.DayScheduleEntry;

namespace CandidContributions.Core.Controllers.Api
{
    //umbraco/api/ScheduleApi
    public class ScheduleApiController : UmbracoApiController
    {
        private readonly List<DaySchedule> _schedule = new List<DaySchedule>();
        private readonly UmbracoHelper _umbracoHelper;

        public ScheduleApiController(UmbracoHelper umbracoHelper)
        {
            _umbracoHelper = umbracoHelper;
        }

        public IEnumerable<CheckBoxViewModel> GetDays([FromRoute]int id)
        {
            SetUpSchedule(id);
            return _schedule.Select(
                s => new CheckBoxViewModel {Text = s.EventStart.ToString("O"), Value = s.DayTitle, Checked = false});
        }

        public IEnumerable<CheckBoxViewModel> GetActivities([FromRoute] int id)
        {
            // only want activities that have entries, not whole list

            SetUpSchedule(id);

            return _schedule
                .SelectMany(x => x.Entries)
                .SelectMany(x => x.Tags)
                .Distinct().Select(x => new CheckBoxViewModel
                {
                    Text = x,
                    Value = x,
                    Checked = false
                })
                .ToList();
        }

        public IEnumerable<DaySchedule> GetSchedule([FromRoute] int id)
        {
            SetUpSchedule(id);
            return _schedule;
        }

        private void SetUpSchedule(int id)
        {
            var eventsPage = (EventsPage)_umbracoHelper.Content(id);
            if (eventsPage == null) return;

            AddDaySchedule(1, eventsPage.Part1StartDate, eventsPage.Part1Entries);
            AddDaySchedule(2, eventsPage.Part2StartDate, eventsPage.Part2Entries);
        }

        private void AddDaySchedule(int part, DateTime startDate, IEnumerable<Umbraco.Cms.Web.Common.PublishedModels.DayScheduleEntry> entries)
        {
            var entriesToShow = entries
                .Where(x => !x.Hidden)
                .OrderBy(x => x.StartTimeUtc).ToList();

            if (startDate == DateTime.MinValue || !entriesToShow.Any()) return;

            // start a new day schedule, start time and duration will be derived from entries
            var day = new DaySchedule($"part{part}", startDate, 0)
            {
                Entries = new List<DayScheduleEntry>()
            };

            foreach (var cmsEntry in entriesToShow)
            {
                var entry = new DayScheduleEntry(startDate, cmsEntry.StartTimeUtc, cmsEntry.DurationMins)
                {
                    Title = cmsEntry.Title, 
                    Abstract = cmsEntry.Abstract.ToHtmlString(),
                    Tags = cmsEntry.Tags
                };

                if (cmsEntry.Speaker is Person person)
                {
                    entry.Speaker = person.FullName;
                    entry.SpeakerAbout = person.About;
                    if (person.Image != null)
                    {
                        entry.SpeakerImage = person.Image.Url();
                    }
                }
                
                day.Entries.Add(entry);
            }

            // set full start date/time and duration from scheduled entries
            var firstEvent = day.Entries.First();
            day.EventStart = new DateTime(firstEvent.Start.Year, firstEvent.Start.Month, firstEvent.Start.Day, firstEvent.Start.Hour, firstEvent.Start.Minute, 0, DateTimeKind.Utc);
            day.DurationInHours = Convert.ToDecimal((double) (day.Entries.Last().End - day.EventStart).TotalHours);

            _schedule.Add(day);
        }
    }
}