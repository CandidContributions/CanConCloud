using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace CandidContributions.Core.Helpers
{
    public static class ViewHelpers
    {
        public static Settings GetRootSettingsNode(this UmbracoHelper umbracoHelper)
        {
            return umbracoHelper.GetSingleByDocType<Settings>(Settings.ModelTypeAlias);
        }

        private static T GetSingleByDocType<T>(this UmbracoHelper umbracoHelper, string docTypeAlias) where T : IPublishedContent
        {
            return (T)umbracoHelper.ContentSingleAtXPath($"//{docTypeAlias}");
        }
        private static T GetSingleByDocType<T>(this UmbracoHelper umbracoHelper, string parentDocTypeAlias, string docTypeAlias) where T : IPublishedContent
        {
            return (T)umbracoHelper.ContentSingleAtXPath($"//{parentDocTypeAlias}//{docTypeAlias}");
        }
    }
}
