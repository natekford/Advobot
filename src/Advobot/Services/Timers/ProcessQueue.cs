using System;
using System.Threading;
using System.Threading.Tasks;
using AdvorangesUtils;

namespace Advobot.Services.Timers
{
	internal sealed class ProcessQueue
	{
		readonly Func<Task> _T;
		readonly SemaphoreSlim _Semaphore;

		public ProcessQueue(int threads, Func<Task> t)
		{
			_Semaphore = new SemaphoreSlim(threads);
			_T = t;
		}

		public void Process()
		{
			if (_Semaphore.CurrentCount <= 0)
			{
				return;
			}

			Task.Run(async () =>
			{
				await _Semaphore.WaitAsync().CAF();
				await _T().CAF();
				_Semaphore.Release();
			});
		}
	}

}