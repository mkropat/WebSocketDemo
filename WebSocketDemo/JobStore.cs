using System.Collections.Concurrent;
using System.Collections.Generic;
using WebSocketDemo.Models;

namespace WebSocketDemo
{
    public class JobStore
    {
        readonly ConcurrentDictionary<string, Job> _jobs = new ConcurrentDictionary<string, Job>();
        readonly object _lock = new object();
        int _nextId = 1;

        public Job CreateJob()
        {
            var job = new Job(GetNextId());
            _jobs[job.Id] = job;
            return job;
        }

        string GetNextId()
        {
            lock (_lock)
                return _nextId++.ToString();
        }

        public Job Find(string id)
        {
            if (!_jobs.ContainsKey(id))
                return null;
            return _jobs[id];
        }

        public IEnumerable<Job> List()
        {
            return _jobs.Values;
        }
    }
}
