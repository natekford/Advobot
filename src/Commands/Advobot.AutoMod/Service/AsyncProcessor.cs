using System;
using System.Threading;
using System.Threading.Tasks;

using AdvorangesUtils;

namespace Advobot.AutoMod
{
	/// <summary>
	/// Does stuff in the background.
	/// </summary>
	public sealed class AsyncProcessor
	{
		private readonly SemaphoreSlim _Semaphore;
		private readonly Func<Task> _T;

		/// <summary>
		/// Creates an instance of <see cref="AsyncProcessor"/>.
		/// </summary>
		/// <param name="count"></param>
		/// <param name="t"></param>
		public AsyncProcessor(int count, Func<Task> t)
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

				try
				{
					await _T().CAF();
				}
				finally
				{
					_Semaphore.Release();
				}
			});
		}
	}
}