using System.Security.Claims;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WebSocketDemo.Models;

namespace WebSocketDemo.Push
{
    public class JobToPushMessageAdapter : IMessageSource
    {
        public event MessageHandler OnMessage = delegate { };

        public JobToPushMessageAdapter(JobStore store)
        {
            store.OnJobUpdated += HandleJobUpdated;
        }

        void HandleJobUpdated(Job oldJob, Job newJob)
        {
            OnMessage(new JobMessage(newJob));
        }

        class JobMessage : IAuthorizableMessage
        {
            public string Message { get; }

            public JobMessage(Job job)
            {
                Message = Serialize(JobResult.FromJob(job));
            }

            public bool IsAuthorized(ClaimsPrincipal principal)
            {
                return true; // TODO: only show jobs relevant to the connected user
            }

            static string Serialize(object obj)
            {
                return JsonConvert.SerializeObject(obj, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                });
            }
        }
    }
}
