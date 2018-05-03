using System.IO;

namespace WebSocketDemo.Models
{
    public class HashRequest
    {
        public Stream Data { get; set; }
        public Job Job { get; set; }

        public override string ToString()
        {
            return $"[HashRequest #{Job?.Id}]";
        }
    }
}
