using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using WebSocketDemo.Models;

namespace WebSocketDemo.Controllers
{
    [Route("api/[controller]")]
    public class JobsController : Controller
    {
        readonly JobStore _store;

        public JobsController(JobStore store)
        {
            _store = store;
        }

        [HttpGet]
        public IEnumerable<JobResult> Get()
        {
            return _store.List()
                .Select(JobResult.FromJob);
        }

        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            var job = _store.Find(id);
            if (job == null)
                return NotFound();
            return Ok(JobResult.FromJob(job));
        }
    }
}
