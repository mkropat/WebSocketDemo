namespace WebSocketDemo.Models
{
    public class Job
    {
        public Job(string id)
        {
            Id = id;
        }

        public string Id { get; }
        public JobStatus Status { get; set; } = JobStatus.New;
        public object Result { get; set; }
    }

    public enum JobStatus
    {
        New,
        Pending,
        Complete,
    }
}
