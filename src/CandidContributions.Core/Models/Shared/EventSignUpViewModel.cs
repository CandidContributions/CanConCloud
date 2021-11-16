using Umbraco.Cms.Core.Strings;

namespace CandidContributions.Core.Models.Shared
{
    public class EventSignUpViewModel
    {
        public IHtmlEncodedString Text { get; set; }

        public string MailchimpGroupId { get; set; }
    }
}