using System.IO;
using Microsoft.AspNetCore.Mvc;
using WebSocketDemo.Models;

namespace WebSocketDemo.Controllers
{
    public delegate void QueueHashJob(HashRequest request);

    [Route("api/[controller]")]
    public class HashController : Controller
    {
        readonly JobStore _jobStore;
        readonly QueueHashJob _queueJob;

        public HashController(JobStore jobStore, QueueHashJob queueHashJob)
        {
            _jobStore = jobStore;
            _queueJob = queueHashJob;
        }

        [HttpPost]
        public JobResult Hash()
        {
            var job = _jobStore.CreateJob();
            _queueJob(new HashRequest
            {
                Data = CopyStream(Request.Body),
                Job = job,
            });
            return JobResult.FromJob(job);
        }

        static Stream CopyStream(Stream source)
        {
            var copy = new MemoryStream();
            source.CopyTo(copy);
            copy.Position = 0;
            return copy;
        }
    }
}
