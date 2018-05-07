using System.Collections.Generic;
using Newtonsoft.Json;

namespace WebSocketDemo.Models
{
    public class JobResult
    {
        public static JobResult FromJob(Job job)
        {
            var result = new JobResult
            {
                Id = job.Id,
                Result = job.Result,
                Status = job.Status.ToString().ToLowerInvariant(),
            };

            result.Links.Add(new HypermediaLink
            {
                Rel = "self",
                Href = $"/api/jobs/{job.Id}",
            });

            return result;
        }

        public string Id { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public object Result { get; set; }

        public string Status { get; set; }

        [JsonProperty(PropertyName = "_links")]
        public List<HypermediaLink> Links { get; } = new List<HypermediaLink>();
    }
}
