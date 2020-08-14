using System;
using System.Threading;
using System.Threading.Tasks;

using AdvorangesUtils;

namespace Advobot.AutoMod
{
	/// <summary>
	/// Uses however many supplied threads to run a separate supplied task on all of them.
	/// </summary>
	public sealed class AsyncProcessingQueue
	{
		private readonly SemaphoreSlim _Semaphore;
		private readonly Func<Task> _T;

		/// <summary>
		/// Creates an instance of <see cref="AsyncProcessingQueue{T}"/>.
		/// </summary>
		/// <param name="count"></param>
		/// <param name="t"></param>
		public AsyncProcessingQueue(int count, Func<Task> t)
		{
			_Semaphore = new SemaphoreSlim(count);
			_T = t;
		}

		/// <summary>
		/// If there are more threads available, starts a new task. Otherwise does nothing.
		/// </summary>
		public void Start()
		{
			_ = Task.Run(async () =>
			{
				await _Semaphore.WaitAsync().CAF();
				await _T().CAF();
				_Semaphore.Release();
			});
		}
	}
}