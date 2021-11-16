using Newtonsoft.Json;

namespace CandidContributions.Core.Models.Api
{
    public class APIResponse
    {
        
        [JsonProperty("response")]
        public EpisodeResponse Response { get; set; }

        
    }
}