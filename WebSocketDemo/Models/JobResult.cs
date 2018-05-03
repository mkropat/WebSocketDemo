using System.Collections.Generic;
using Newtonsoft.Json;

namespace WebSocketDemo.Models
{
    public class JobResult
    {
        public static JobResult FromJob(Job job)
        {
            var result = new JobResult();
            result.Result = job.Result;
            result.Status = job.Status.ToString().ToLowerInvariant();

            result.Links.Add(new HypermediaLink
            {
                Rel = "self",
                Href = $"/api/jobs/{job.Id}",
            });

            return result;
        }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public object Result { get; set; }

        public string Status { get; set; }

        [JsonProperty(PropertyName = "_links")]
        public List<HypermediaLink> Links { get; } = new List<HypermediaLink>();
    }
}
