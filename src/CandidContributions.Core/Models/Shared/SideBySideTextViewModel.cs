using Umbraco.Cms.Core.Strings;

namespace CandidContributions.Core.Models.Shared
{
    public class SideBySideTextViewModel
    {
        public IHtmlEncodedString TextLeft { get; set; }
        public IHtmlEncodedString TextRight { get; set; }
    }
}