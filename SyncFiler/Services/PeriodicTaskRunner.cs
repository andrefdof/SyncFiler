using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncFiler.Services
{
    public class PeriodicTaskRunner
    {
        public TimeSpan Interval { get; private set; }
        private CancellationTokenSource? _cancellationTokenSource;

        public PeriodicTaskRunner(TimeSpan interval)
        {
            Interval = interval;
        }

        public async Task StartAsync(Func<Task> taskToRun, CancellationToken cancellationToken = default)
        {
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var token = _cancellationTokenSource.Token;

            while (!token.IsCancellationRequested)
            {
                await taskToRun();
                await Task.Delay(Interval, token);
            }
        }
    }
}
