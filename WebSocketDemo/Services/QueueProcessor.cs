using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebSocketDemo.Services
{
    public abstract class QueueProcessor<T> : IHostedService
    {
        readonly TimeSpan _pollDelay = TimeSpan.FromMilliseconds(100);

        Task _backgroundTask;
        readonly ILogger _log;
        readonly CancellationTokenSource _shutdown = new CancellationTokenSource();
        readonly IProducerConsumerCollection<T> _workQueue;

        public QueueProcessor(IProducerConsumerCollection<T> workQueue, ILogger logger)
        {
            _log = logger;
            _workQueue = workQueue;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _backgroundTask = Task.Run(ProcessQueue, cancellationToken);
            return Task.CompletedTask;
        }

        async Task ProcessQueue()
        {
            while (!_shutdown.IsCancellationRequested)
            {
                if (!_workQueue.TryTake(out var request))
                {
                    await Task.Delay(_pollDelay, _shutdown.Token);
                    continue;
                }

                try
                {
                    await HandleRequest(request, _shutdown.Token);
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    _log.LogError(ex, $"Error handling: {request}");
                }
            }
        }

        protected abstract Task HandleRequest(T request, CancellationToken cancelToken);

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _shutdown.Cancel();
            return Task.WhenAny(_backgroundTask, Task.Delay(Timeout.Infinite, cancellationToken));
        }
    }
}
