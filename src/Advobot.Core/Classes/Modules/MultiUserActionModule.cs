using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;

namespace Advobot.Classes.Modules
{
	/// <summary>
	/// Does an action on all the input users until either no more users remain or the cancel token has been canceled.
	/// </summary>
	public abstract class MultiUserActionModule : AdvobotModuleBase
	{
		private static readonly ConcurrentDictionary<ulong, CancellationTokenSource> _CancelTokens = new ConcurrentDictionary<ulong, CancellationTokenSource>();

		/// <summary>
		/// Logs progress when a user is modified.
		/// </summary>
		protected IAsyncProgress<MultiUserActionProgressArgs>? ProgressLogger { get; set; }

		/// <summary>
		/// Does an action on many users at once.
		/// </summary>
		/// <param name="bypass"></param>
		/// <param name="predicate"></param>
		/// <param name="update"></param>
		/// <returns></returns>
		protected Task<int> ProcessAsync(bool bypass, Func<IGuildUser, bool> predicate, Func<IGuildUser, Task> update)
			=> ProcessAsync(Context.Guild.GetEditableUsers(Context.User), bypass, predicate, update);
		/// <summary>
		/// Does an action on many users at once.
		/// </summary>
		/// <param name="users"></param>
		/// <param name="bypass"></param>
		/// <param name="predicate"></param>
		/// <param name="update"></param>
		/// <returns></returns>
		protected Task<int> ProcessAsync(IEnumerable<IGuildUser> users, bool bypass, Func<IGuildUser, bool> predicate, Func<IGuildUser, Task> update)
		{
			var amount = bypass ? int.MaxValue : BotSettings.MaxUserGatherCount;
			var array = users.Where(predicate).Take(amount).ToArray();
			return ProcessAsync(array, update);
		}
		/// <summary>
		/// Does an action on many users at once.
		/// </summary>
		/// <param name="users"></param>
		/// <param name="update"></param>
		/// <returns></returns>
		protected async Task<int> ProcessAsync(IReadOnlyCollection<IGuildUser> users, Func<IGuildUser, Task> update)
		{
			var cancelToken = new CancellationTokenSource();
			_CancelTokens.AddOrUpdate(Context.Guild.Id, cancelToken, (oldKey, oldValue) =>
			{
				oldValue?.Cancel();
				return cancelToken;
			});

			var successCount = 0;
			for (var i = 0; i < users.Count; ++i)
			{
				if (cancelToken.IsCancellationRequested)
				{
					break;
				}
				if (ProgressLogger != null)
				{
					await ProgressLogger.ReportAsync(new MultiUserActionProgressArgs(users.Count, i + 1)).CAF();
				}

				await update(users.ElementAt(i)).CAF();
				++successCount;
			}
			return successCount;
		}

		/// <summary>
		/// Event arguments for the status of the multi user action.
		/// </summary>
		public class MultiUserActionProgressArgs : EventArgs
		{
			/// <summary>
			/// The total amount of users this is currently targetting.
			/// </summary>
			public int TotalUsers { get; }
			/// <summary>
			/// The amount of users this has already modified.
			/// </summary>
			public int CurrentProgress { get; }
			/// <summary>
			/// The amount of users left to modify.
			/// </summary>
			public int AmountLeft => TotalUsers - CurrentProgress;
			/// <summary>
			/// Whether this is the start of the multi user action.
			/// </summary>
			public bool IsStart => CurrentProgress == 0;
			/// <summary>
			/// Whether this is the end of the multi user action.
			/// </summary>
			public bool IsEnd => CurrentProgress == TotalUsers;

			/// <summary>
			/// Creates an instance of <see cref="MultiUserActionProgressArgs"/>.
			/// </summary>
			/// <param name="total"></param>
			/// <param name="current"></param>
			public MultiUserActionProgressArgs(int total, int current)
			{
				TotalUsers = total;
				CurrentProgress = current;
			}
		}

		/// <summary>
		/// Logs progress for multi user actions.
		/// </summary>
		public class MultiUserActionProgressLogger : IAsyncProgress<MultiUserActionProgressArgs>
		{
			private readonly IMessageChannel _Channel;
			private readonly Func<MultiUserActionProgressArgs, string> _CreateResult;
			private readonly RequestOptions? _Options;
			private IUserMessage? message = null;

			/// <summary>
			/// Creates an instance of <see cref="MultiUserActionProgressLogger"/>.
			/// </summary>
			/// <param name="channel"></param>
			/// <param name="createResult"></param>
			/// <param name="options"></param>
			public MultiUserActionProgressLogger(IMessageChannel channel, Func<MultiUserActionProgressArgs, string> createResult, RequestOptions? options = null)
			{
				_Channel = channel ?? throw new ArgumentNullException(nameof(channel));
				_CreateResult = createResult ?? throw new ArgumentNullException(nameof(createResult));
				_Options = options;
			}

			/// <summary>
			/// Logs the information passed in to the channel and updates a message.
			/// </summary>
			/// <param name="value"></param>
			/// <returns></returns>
			public async Task ReportAsync(MultiUserActionProgressArgs value)
			{
				//var newText = ;
				if (value.IsStart)
				{
					message = await _Channel.SendMessageAsync(_CreateResult(value), options: _Options).CAF();
				}
				else if (value.IsEnd && message != null)
				{
					await message.DeleteAsync(_Options).CAF();
				}
				else if (value.CurrentProgress % 10 == 0 && message != null)
				{
					await message.ModifyAsync(x => x.Content = _CreateResult(value), _Options).CAF();
				}
			}
		}
	}
}
