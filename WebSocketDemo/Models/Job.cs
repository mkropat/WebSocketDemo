namespace WebSocketDemo.Models
{
    public class Job
    {
        public Job(string id) : this(id, null, JobStatus.New) { }

        Job(string id, object result, JobStatus status)
        {
            Id = id;
            Result = result;
            Status = status;
        }

        public string Id { get; }
        public object Result { get; }
        public JobStatus Status { get; }

        public Job UpdateStatus(JobStatus status)
        {
            return new Job(Id, Result, status);
        }

        public Job SetResult(object result)
        {
            return new Job(Id, result, Status);
        }
    }

    public enum JobStatus
    {
        New,
        Pending,
        Complete,
    }
}
