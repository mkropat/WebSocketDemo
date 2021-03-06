﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using WebSocketDemo.Models;

namespace WebSocketDemo
{
    public delegate void JobUpdateHandler(Job oldJob, Job newJob);

    public class JobStore
    {
        public event JobUpdateHandler OnJobUpdated = delegate { };

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

        public void Update(Job newJob)
        {
            if (string.IsNullOrEmpty(newJob.Id) || !_jobs.ContainsKey(newJob.Id))
                throw new ArgumentException("Job ID does not exist. Did you use CreateJob?", nameof(newJob));

            var oldJob = _jobs[newJob.Id];
            _jobs[newJob.Id] = newJob;
            OnJobUpdated(oldJob, newJob);
        }

        public IEnumerable<Job> List()
        {
            return _jobs.Values;
        }
    }
}
