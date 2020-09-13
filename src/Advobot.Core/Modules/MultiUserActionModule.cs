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

namespace Advobot.Modules
{
	/// <summary>
	/// Does an action on all the input users until either no more users remain or the cancel token has been canceled.
	/// </summary>
	public abstract class MultiUserActionModule : AdvobotModuleBase
	{
		private static readonly ConcurrentDictionary<ulong, CancellationTokenSource> _CancelTokens
			= new ConcurrentDictionary<ulong, CancellationTokenSource>();

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
		/// <param name="options"></param>
		/// <returns></returns>
		protected Task<int> ProcessAsync(
			bool bypass,
			Func<IGuildUser, bool> predicate,
			Func<IGuildUser, RequestOptions, Task> update,
			RequestOptions options)
		{
			var users = Context.Guild.Users.Where(CanBeModified);
			return ProcessAsync(users, bypass, predicate, update, options);
		}

		/// <summary>
		/// Does an action on many users at once.
		/// </summary>
		/// <param name="users"></param>
		/// <param name="bypass"></param>
		/// <param name="predicate"></param>
		/// <param name="update"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		protected Task<int> ProcessAsync(
			IEnumerable<IGuildUser> users,
			bool bypass,
			Func<IGuildUser, bool> predicate,
			Func<IGuildUser, RequestOptions, Task> update,
			RequestOptions options)
		{
			var amount = bypass ? int.MaxValue : BotSettings.MaxUserGatherCount;
			var array = users.Where(predicate).Take(amount).ToArray();
			return ProcessAsync(array, update, options);
		}

		private bool CanBeModified(IGuildUser user)
			=> Context.User.CanModify(user) && Context.Guild.CurrentUser.CanModify(user);

		private async Task<int> ProcessAsync(
			IReadOnlyList<IGuildUser> users,
			Func<IGuildUser, RequestOptions, Task> update,
			RequestOptions options)
		{
			var token = new CancellationTokenSource();
			_CancelTokens.AddOrUpdate(Context.Guild.Id, token, (_, v) =>
			{
				v?.Cancel();
				return token;
			});

			var i = 0;
			for (; i < users.Count; ++i)
			{
				if (token.IsCancellationRequested)
				{
					break;
				}
				else if (ProgressLogger != null)
				{
					var args = new MultiUserActionProgressArgs(users.Count, i + 1);
					await ProgressLogger.ReportAsync(args).CAF();
				}

				await update(users[i], options).CAF();
			}
			return i;
		}

		/// <summary>
		/// Event arguments for the status of the multi user action.
		/// </summary>
		public class MultiUserActionProgressArgs : EventArgs
		{
			/// <summary>
			/// The amount of users left to modify.
			/// </summary>
			public int AmountLeft => TotalUsers - CurrentProgress;

			/// <summary>
			/// The amount of users this has already modified.
			/// </summary>
			public int CurrentProgress { get; }

			/// <summary>
			/// Whether this is the end of the multi user action.
			/// </summary>
			public bool IsEnd => CurrentProgress == TotalUsers;

			/// <summary>
			/// Whether this is the start of the multi user action.
			/// </summary>
			public bool IsStart => CurrentProgress == 0;

			/// <summary>
			/// The total amount of users this is currently targetting.
			/// </summary>
			public int TotalUsers { get; }

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
			private bool HasException;
			private IUserMessage? message;

			/// <summary>
			/// Creates an instance of <see cref="MultiUserActionProgressLogger"/>.
			/// </summary>
			/// <param name="channel"></param>
			/// <param name="createResult"></param>
			/// <param name="options"></param>
			public MultiUserActionProgressLogger(
				IMessageChannel channel,
				Func<MultiUserActionProgressArgs, string> createResult,
				RequestOptions? options = null)
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
				try
				{
					if (HasException)
					{
						return;
					}
					else if (value.IsStart)
					{
						message = await _Channel.SendMessageAsync(new MessageArgs
						{
							Content = _CreateResult(value),
							Options = _Options,
						}).CAF();
					}
					else if (message == null)
					{
						return;
					}
					else if (value.IsEnd)
					{
						await message.DeleteAsync(_Options).CAF();
					}
					else if (value.CurrentProgress % 10 == 0)
					{
						await message.ModifyAsync(x => x.Content = _CreateResult(value), _Options).CAF();
					}
				}
				catch (Exception e)
				{
					e.Write();
					HasException = true;
				}
			}
		}
	}
}