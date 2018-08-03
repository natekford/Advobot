using System;
using System.Threading;
using System.Threading.Tasks;
using AdvorangesUtils;

namespace Advobot.Services.Timers
{
	/// <summary>
	/// Uses however many supplied threads to run a separate supplied task on all of them.
	/// </summary>
	internal sealed class ProcessingQueue
	{
		private readonly Func<Task> _T;
		private readonly SemaphoreSlim _Semaphore;

		/// <summary>
		/// Creates an instance of <see cref="ProcessingQueue"/>.
		/// </summary>
		/// <param name="threads"></param>
		/// <param name="t"></param>
		public ProcessingQueue(int threads, Func<Task> t)
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