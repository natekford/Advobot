using Advobot.Utilities;

using AdvorangesUtils;

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
	protected Task<int> ProcessAsync(
		bool getUnlimitedUsers,
		Func<IGuildUser, bool> userPredicate,
		Func<IGuildUser, RequestOptions, Task> updateUser,
		Func<MultiUserActionProgressArgs, string>? formatProgress,
		RequestOptions options)
	{
		var users = Context.Guild.Users
			.Where(x => Context.User.CanModify(x) && Context.Guild.CurrentUser.CanModify(x));
		return ProcessAsync(users, getUnlimitedUsers, userPredicate, updateUser, formatProgress, options);
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
		var amount = getUnlimitedUsers ? int.MaxValue : BotSettings.MaxUserGatherCount;
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

		var hasException = false;
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
					if (hasException)
					{
						continue;
					}
					else if (args.IsStart)
					{
						message = await Context.Channel.SendMessageAsync(new SendMessageArgs
						{
							Content = formatProgress(args),
							Options = options,
						}).CAF();
					}
					else if (message is null)
					{
						continue;
					}
					else if (args.IsEnd)
					{
						await message.DeleteAsync(options).CAF();
					}
					else if (args.CurrentProgress % 10 == 0)
					{
						await message.ModifyAsync(x => x.Content = formatProgress(args), options).CAF();
					}
				}
				catch (Exception e)
				{
					e.Write();
					hasException = true;
				}
			}

			await update(users[i], options).CAF();
		}
		return i;
	}

	/// <summary>
	/// Event arguments for the status of the multi user action.
	/// </summary>
	/// <remarks>
	/// Creates an instance of <see cref="MultiUserActionProgressArgs"/>.
	/// </remarks>
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