using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.WebSocket;

namespace Advobot.Classes.Modules
{
	/// <summary>
	/// Does an action on all the input users until either no more users remain or the cancel token has been canceled.
	/// </summary>
	public abstract class MultiUserActionModule : AdvobotModuleBase
	{
		private static readonly ConcurrentDictionary<ulong, CancellationTokenSource> _CancelTokens = new ConcurrentDictionary<ulong, CancellationTokenSource>();

		/// <summary>
		/// Does an action on many users at once.
		/// </summary>
		/// <param name="bypass"></param>
		/// <param name="predicate"></param>
		/// <param name="update"></param>
		/// <returns></returns>
		public async Task Process(
			bool bypass,
			Func<SocketGuildUser, bool> predicate,
			Func<SocketGuildUser, RequestOptions, Task> update)
			=> await ProcessAsync(Context.Guild.GetEditableUsers(Context.User), bypass, predicate, update).CAF();
		/// <summary>
		/// Does an action on many users at once.
		/// </summary>
		/// <param name="users"></param>
		/// <param name="bypass"></param>
		/// <param name="predicate"></param>
		/// <param name="update"></param>
		/// <returns></returns>
		public async Task ProcessAsync(
			IEnumerable<SocketGuildUser> users,
			bool bypass,
			Func<SocketGuildUser, bool> predicate,
			Func<SocketGuildUser, RequestOptions, Task> update)
		{
			var cancelToken = new CancellationTokenSource();
			_CancelTokens.AddOrUpdate(Context.Guild.Id, cancelToken, (oldKey, oldValue) =>
			{
				oldValue?.Cancel();
				return cancelToken;
			});

			var amount = bypass ? int.MaxValue : BotSettings.MaxUserGatherCount;
			var array = users.Where(predicate).Take(amount).ToArray();
			var text = $"Attempting to modify `{array.Length}` users.";
			var message = await ReplyAsync(text).CAF();
			var options = GenerateRequestOptions();

			var successCount = 0;
			for (var i = 0; i < array.Length; ++i)
			{
				if (cancelToken.IsCancellationRequested)
				{
					break;
				}
				if (i % 10 == 0)
				{
					var amtLeft = array.Length - i;
					var time = (int)(amtLeft * 1.2);
					var newText = $"Attempting to modify `{amtLeft}` users. ETA on completion: `{time}` seconds.";
					await message.ModifyAsync(x => x.Content = newText).CAF();
				}

				++successCount;
				var user = array[i];
				try
				{
					await update(user, options).CAF();
				}
				//Lots of potential for things to go wrong, but they should all be completely ignored
				catch { }
			}

			await message.DeleteAsync(options).CAF();
			await ReplyAsync($"Successfully modified `{successCount}` users.").CAF();
		}
	}
}
