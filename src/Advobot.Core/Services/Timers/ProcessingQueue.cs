using System;
using System.Threading;
using System.Threading.Tasks;

using AdvorangesUtils;

namespace Advobot.Services.Timers
{
	/// <summary>
	/// Uses however many supplied threads to run a separate supplied task on all of them.
	/// </summary>
	internal sealed class AsyncProcessingQueue
	{
		private readonly SemaphoreSlim _Semaphore;
		private readonly Func<Task> _T;

		/// <summary>
		/// Creates an instance of <see cref="AsyncProcessingQueue"/>.
		/// </summary>
		/// <param name="threads"></param>
		/// <param name="t"></param>
		public AsyncProcessingQueue(int threads, Func<Task> t)
		{
			_Semaphore = new SemaphoreSlim(threads);
			_T = t;
		}

		/// <summary>
		/// If there are more threads available, starts a new task. Otherwise does nothing.
		/// </summary>
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