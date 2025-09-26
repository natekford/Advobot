using Advobot.Utilities;

using Discord;

using System.Collections.Concurrent;

namespace Advobot.Modules;

/// <summary>
/// Does an action on all the input users until either no more users remain or the cancel token has been canceled.
/// </summary>
public abstract class MultiUserActionModuleBase : AdvobotModuleBase
{
	private static readonly ConcurrentDictionary<ulong, CancellationTokenSource> _CancelTokens = new();

	/// <summary>
	/// Does an action on many users at once.
	/// </summary>
	/// <param name="getUnlimitedUsers"></param>
	/// <param name="userPredicate"></param>
	/// <param name="updateUser"></param>
	/// <param name="formatProgress"></param>
	/// <param name="options"></param>
	/// <returns></returns>
	protected async Task<int> ProcessAsync(
		bool getUnlimitedUsers,
		Func<IGuildUser, bool> userPredicate,
		Func<IGuildUser, RequestOptions, Task> updateUser,
		Func<MultiUserActionProgressArgs, string>? formatProgress,
		RequestOptions options)
	{
		var bot = await Context.Guild.GetCurrentUserAsync().ConfigureAwait(false);
		var users = await Context.Guild.GetUsersAsync().ConfigureAwait(false);
		var modifiable = users.Where(x => Context.User.CanModify(x) && bot.CanModify(x));
		return await ProcessAsync(modifiable, getUnlimitedUsers, userPredicate, updateUser, formatProgress, options).ConfigureAwait(false);
	}

	/// <summary>
	/// Does an action on many users at once.
	/// </summary>
	/// <param name="users"></param>
	/// <param name="getUnlimitedUsers"></param>
	/// <param name="userPredicate"></param>
	/// <param name="updateUser"></param>
	/// <param name="formatProgress"></param>
	/// <param name="options"></param>
	/// <returns></returns>
	protected Task<int> ProcessAsync(
		IEnumerable<IGuildUser> users,
		bool getUnlimitedUsers,
		Func<IGuildUser, bool> userPredicate,
		Func<IGuildUser, RequestOptions, Task> updateUser,
		Func<MultiUserActionProgressArgs, string>? formatProgress,
		RequestOptions options)
	{
		var amount = getUnlimitedUsers ? int.MaxValue : BotConfig.MaxUserGatherCount;
		var array = users.Where(userPredicate).Take(amount).ToArray();
		return ProcessAsync(array, updateUser, formatProgress, options);
	}

	private async Task<int> ProcessAsync(
		IGuildUser[] users,
		Func<IGuildUser, RequestOptions, Task> update,
		Func<MultiUserActionProgressArgs, string>? formatProgress,
		RequestOptions options)
	{
		var token = new CancellationTokenSource();
		_CancelTokens.AddOrUpdate(Context.Guild.Id, token, (_, v) =>
		{
			v?.Cancel();
			return token;
		});

		var message = default(IUserMessage);
		var i = 0;
		for (; i < users.Length; ++i)
		{
			if (token.IsCancellationRequested)
			{
				break;
			}
			else if (formatProgress != null)
			{
				var args = new MultiUserActionProgressArgs(users.Length, i + 1);
				try
				{
					if (args.IsStart)
					{
						message = await Context.Channel.SendMessageAsync(new SendMessageArgs
						{
							Content = formatProgress(args),
							Options = options,
						}).ConfigureAwait(false);
					}
					else if (message is not null && args.IsEnd)
					{
						await message.DeleteAsync(options).ConfigureAwait(false);
					}
					else if (message is not null && args.CurrentProgress % 10 == 0)
					{
						await message.ModifyAsync(x => x.Content = formatProgress(args), options).ConfigureAwait(false);
					}
				}
				catch (Exception e)
				{
					await Context.Channel.SendMessageAsync(new SendMessageArgs
					{
						Content = $"An error occurred: {e.Message}",
						Options = options,
					}).ConfigureAwait(false);
					token.Cancel();
					return i;
				}
			}

			await update(users[i], options).ConfigureAwait(false);
		}
		return i;
	}

	/// <summary>
	/// Event arguments for the status of the multi user action.
	/// </summary>
	/// <param name="total"></param>
	/// <param name="current"></param>
	public sealed class MultiUserActionProgressArgs(int total, int current) : EventArgs
	{
		/// <summary>
		/// The amount of users left to modify.
		/// </summary>
		public int AmountLeft => TotalUsers - CurrentProgress;
		/// <summary>
		/// The amount of users this has already modified.
		/// </summary>
		public int CurrentProgress { get; } = current;
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
		public int TotalUsers { get; } = total;
	}
}