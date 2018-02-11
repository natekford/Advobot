using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Advobot.Core.Classes
{
	public abstract class ImageCreationModuleBase<T> : NonSavingModuleBase where T : IImageResizerArguments
	{
		private static ConcurrentQueue<ImageCreationArguments<T>> _Args = new ConcurrentQueue<ImageCreationArguments<T>>();
		private static ConcurrentDictionary<ulong, object> _CurrentlyWorkingGuilds = new ConcurrentDictionary<ulong, object>();
		private static SemaphoreSlim _SemaphoreSlim = new SemaphoreSlim(4);

		protected bool GuildAlreadyProcessing => _Args.Any(x => x.Context.Guild.Id == Context.Guild.Id) || _CurrentlyWorkingGuilds.Any(x => x.Key == Context.Guild.Id);
		protected bool CanStart => _SemaphoreSlim.CurrentCount > 0;
		protected int QueueCount => _Args.Count;

		protected void EnqueueArguments(ImageCreationArguments<T> args)
		{
			_Args.Enqueue(args);
		}
		protected void StartProcessing()
		{
			//Store it as a variable to get rid of the warning and allow it to run on its own
			Task.Run(async () =>
			{
				//Lock since only a few threads should be processing this at once
				await _SemaphoreSlim.WaitAsync().CAF();
				while (_Args.TryDequeue(out var dequeued))
				{
					_CurrentlyWorkingGuilds.AddOrUpdate(dequeued.Context.Guild.Id, new object(), (k, v) => new object());
					await Create(dequeued).CAF();
					_CurrentlyWorkingGuilds.TryRemove(dequeued.Context.Guild.Id, out var removed);
				}
				_SemaphoreSlim.Release();
			}).CAF();
		}
		protected abstract Task Create(ImageCreationArguments<T> args);
	}
}
