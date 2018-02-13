using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Discord;
using ImageMagick;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Runs image resizing in background threads. The arguments get enqueued, then the image is resized, and finally the callback is invoked in order to use the resized image.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ImageResizer<T> where T : IImageResizerArguments
    {
		private ConcurrentQueue<ImageCreationArguments> _Args = new ConcurrentQueue<ImageCreationArguments>();
		private ConcurrentDictionary<ulong, object> _CurrentlyWorkingGuilds = new ConcurrentDictionary<ulong, object>();
		private SemaphoreSlim _SemaphoreSlim;
		private Func<AdvobotSocketCommandContext, MemoryStream, MagickFormat, string, RequestOptions, Task<Error>> _Callback;
		private string _Type;
		/// <summary>
		/// Returns true if there are any available threads to run.
		/// </summary>
		public bool CanStart => _SemaphoreSlim.CurrentCount > 0;
		/// <summary>
		/// Returns the current amount of items in the queue.
		/// </summary>
		public int QueueCount => _Args.Count;

		/// <summary>
		/// Resizes images and then uses the callback.
		/// </summary>
		/// <param name="threads">How many threads to run in the background.</param>
		/// <param name="type">Guild icon, bot icon, emote, webhook icon, etc. (only used in response messages, nothing important)</param>
		/// <param name="callback">What to do with the resized stream.</param>
		public ImageResizer(int threads, string type, Func<AdvobotSocketCommandContext, MemoryStream, MagickFormat, string, RequestOptions, Task<Error>> callback)
		{
			_SemaphoreSlim = new SemaphoreSlim(threads);
			_Type = type;
			_Callback = callback;
		}

		/// <summary>
		/// Returns true if the image is already being processed.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		public bool IsGuildAlreadyProcessing(IGuild guild)
		{
			return _Args.Any(x => x.Context.Guild.Id == guild.Id) || _CurrentlyWorkingGuilds.Any(x => x.Key == guild.Id);
		}
		/// <summary>
		/// Add the arguments to the queue in order to be resized.
		/// </summary>
		/// <param name="context">The current context.</param>
		/// <param name="args">The arguments to use when resizing the image.</param>
		/// <param name="uri">The url to download from.</param>
		/// <param name="options">The request options to use in the callback.</param>
		/// <param name="nameOrId">The name for an emote, or the id for a webhook.</param>
		public void EnqueueArguments(AdvobotSocketCommandContext context, T args, Uri uri, RequestOptions options, string nameOrId = null)
		{
			_Args.Enqueue(new ImageCreationArguments
			{
				Context = context,
				Args = args,
				Uri = uri,
				Options = options,
				NameOrId = nameOrId,
			});
		}
		/// <summary>
		/// If there are any threads available, this will start another thread processing image resizing and callback usage.
		/// </summary>
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
					using (var resp = await ImageUtils.ResizeImageAsync(d.Uri, d.Context, d.Args).CAF())
					{
						if (!resp.IsSuccess)
						{
							await MessageUtils.SendErrorMessageAsync(d.Context, new Error($"Failed to create the {_Type}. Reason: {resp.Error}.")).CAF();
						}
						else if (await _Callback(d.Context, resp.Stream, resp.Format, d.NameOrId, d.Options).CAF() is Error error)
						{
							await MessageUtils.SendErrorMessageAsync(d.Context, error).CAF();
						}
						else
						{
							await MessageUtils.MakeAndDeleteSecondaryMessageAsync(d.Context, $"Successfully created the {_Type}.").CAF();
						}
					}
					_CurrentlyWorkingGuilds.TryRemove(d.Context.Guild.Id, out var removed);
				}
				_SemaphoreSlim.Release();
			}).CAF();
		}

		private struct ImageCreationArguments
		{
			public AdvobotSocketCommandContext Context;
			public T Args;
			public Uri Uri;
			public RequestOptions Options;
			public string NameOrId;
		}
	}
}
