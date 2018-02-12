using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Discord;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Advobot.Core.Classes
{
	public abstract class ImageResizer<T> where T : IImageResizerArguments
    {
		private static ConcurrentQueue<ImageCreationArguments> _Args = new ConcurrentQueue<ImageCreationArguments>();
		private static ConcurrentDictionary<ulong, object> _CurrentlyWorkingGuilds = new ConcurrentDictionary<ulong, object>();
		private static SemaphoreSlim _SemaphoreSlim;

		public ImageResizer(int threads)
		{
			_SemaphoreSlim = new SemaphoreSlim(threads);
		}

		public bool CanStart => _SemaphoreSlim.CurrentCount > 0;
		public int QueueCount => _Args.Count;

		public bool IsGuildAlreadyProcessing(IGuild guild)
		{
			return _Args.Any(x => x.Context.Guild.Id == guild.Id) || _CurrentlyWorkingGuilds.Any(x => x.Key == guild.Id);
		}
		public void EnqueueArguments(AdvobotSocketCommandContext context, T args, Uri uri, RequestOptions options, string nameForEmote = null)
		{
			_Args.Enqueue(new ImageCreationArguments
			{
				Context = context,
				Args = args,
				Uri = uri,
				Options = options,
				NameForEmote = nameForEmote,
			});
		}
		public void StartProcessing()
		{
			//Store it as a variable to get rid of the warning and allow it to run on its own
			Task.Run(async () =>
			{
				//Lock since only a few threads should be processing this at once
				await _SemaphoreSlim.WaitAsync().CAF();
				while (_Args.TryDequeue(out var d))
				{
					_CurrentlyWorkingGuilds.AddOrUpdate(d.Context.Guild.Id, new object(), (k, v) => new object());
					await Create(d.Context, d.Args, d.Uri, d.Options, d.NameForEmote).CAF();
					_CurrentlyWorkingGuilds.TryRemove(d.Context.Guild.Id, out var removed);
				}
				_SemaphoreSlim.Release();
			}).CAF();
		}
		protected abstract Task Create(AdvobotSocketCommandContext context, T args, Uri uri, RequestOptions options, string nameForEmote);

		private struct ImageCreationArguments
		{
			public AdvobotSocketCommandContext Context;
			public T Args;
			public Uri Uri;
			public RequestOptions Options;
			public string NameForEmote;
		}
	}
}
