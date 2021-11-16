using Umbraco.Cms.Core.Strings;

namespace CandidContributions.Core.Models.Forms
{
    public class EventSignupForm
    {
        public string MailchimpGroupId { get; set; }

        public string Email { get; set; }

        public string First { get; set; }

        public string Last { get; set; }

        // signup info text from CMS
        public IHtmlEncodedString Text { get; set; }
    }
}
