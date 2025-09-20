using Advobot.Interactivity;
using Advobot.Interactivity.Criterions;
using Advobot.Interactivity.TryParsers;
using Advobot.Preconditions;
using Advobot.Services.BotConfig;
using Advobot.Services.Events;
using Advobot.Services.Punishments;
using Advobot.Utilities;

using Discord;

using YACCS.Commands;
using YACCS.Commands.Building;

namespace Advobot.Modules;

/// <summary>
/// Shorter way to write the used modulebase and also has every command go through the <see cref="ExtendableCommandValidation"/> first.
/// </summary>
[ExtendableCommandValidation]
public abstract class AdvobotModuleBase : CommandGroup<IGuildContext>
{
	private static readonly ICriterion<IMessage>[] _NextIndexCriteria =
	[
		new EnsureSourceChannelCriterion(),
		new EnsureSourceUserCriterion(),
	];

	/// <summary>
	/// The settings for the bot.
	/// </summary>
	[InjectService]
	public required IRuntimeConfig BotConfig { get; set; }
	/// <summary>
	/// The service to provide events with.
	/// </summary>
	[InjectService]
	public required EventProvider EventProvider { get; set; }
	/// <summary>
	/// The service to use for giving punishments.
	/// </summary>
	[InjectService]
	public required IPunishmentService PunishmentService { get; set; }
	/// <summary>
	/// The default time to wait for a user's response.
	/// </summary>
	protected TimeSpan DefaultInteractivityTime { get; set; } = TimeSpan.FromSeconds(5);

	/// <summary>
	/// Gets a <see cref="RequestOptions"/> that mainly is used for the reason in the audit log.
	/// </summary>
	/// <param name="reason"></param>
	/// <returns></returns>
	public RequestOptions GetOptions(string? reason = null)
	{
		var r = Context.User.Format();
		if (reason != null)
		{
			r += $": {reason.TrimEnd()}.";
		}
		return DiscordUtils.GenerateRequestOptions(r);
	}

	/// <summary>
	/// Gets a user to display.
	/// </summary>
	/// <param name="userId"></param>
	/// <returns></returns>
	public Task<IUser?> GetUserAsync(ulong userId)
		=> Context.Client.GetUserAsync(userId);

	/// <summary>
	/// Gets the next valid index supplied by the user. This is blocking.
	/// </summary>
	/// <param name="minVal"></param>
	/// <param name="maxVal"></param>
	/// <param name="options"></param>
	/// <returns></returns>
	public Task<InteractiveResult<int>> NextIndexAsync(
		int minVal,
		int maxVal,
		InteractivityOptions? options = null)
	{
		var tryParser = new IndexTryParser(minVal, maxVal);
		options ??= new();
		options.Criteria = _NextIndexCriteria;
		return NextValueAsync(tryParser, options);
	}

	/// <summary>
	/// Uses user input to get the item at a specified index. This is blocking.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="source"></param>
	/// <param name="format"></param>
	/// <param name="options"></param>
	/// <returns></returns>
	public async Task<InteractiveResult<T>> NextItemAtIndexAsync<T>(
		IReadOnlyList<T> source,
		Func<T, string> format,
		InteractivityOptions? options = null)
	{
		var list = source.Select(format).FormatNumberedList();
		var message = await ReplyAsync($"Did you mean any of the following:\n{list}").ConfigureAwait(false);
		var index = await NextIndexAsync(0, source.Count - 1, options).ConfigureAwait(false);
		await message.DeleteAsync(GetOptions()).ConfigureAwait(false);
		return index.HasValue ? source[index.Value] : InteractiveResult<T>.PropagateError(index);
	}

	/// <summary>
	/// Gets the next message which makes <paramref name="tryParser"/> return true. This is blocking.
	/// </summary>
	/// <param name="tryParser"></param>
	/// <param name="options"></param>
	/// <returns></returns>
	/// <remarks>Heavily taken from https://github.com/foxbot/Discord.Addons.Interactive/blob/master/Discord.Addons.Interactive/InteractiveService.cs</remarks>
	public Task<InteractiveResult<T>> NextValueAsync<T>(
		IMessageTryParser<T> tryParser,
		InteractivityOptions? options = null)
	{
		throw new NotImplementedException();
		/*
		var timeout = options?.Timeout ?? DefaultInteractivityTime;

		var eventTrigger = new TaskCompletionSource<T>();
		var cancelTrigger = new TaskCompletionSource<bool>();
		if (options?.Token is CancellationToken token)
		{
			token.Register(() => cancelTrigger.SetResult(true));
		}

		var criteria = options?.Criteria ?? [];
		async Task Handler(IMessage message)
		{
			foreach (var criterion in criteria)
			{
				var result = await criterion.JudgeAsync(Context, message).ConfigureAwait(false);
				if (!result)
				{
					return;
				}
			}

			var parsed = await tryParser.TryParseAsync(message).ConfigureAwait(false);
			if (parsed.IsSpecified)
			{
				eventTrigger.SetResult(parsed.Value);
			}
		}

		EventProvider.MessageReceived.Add(Handler);
		var @event = eventTrigger.Task;
		var cancel = cancelTrigger.Task;
		var delay = Task.Delay(timeout);
		var task = await Task.WhenAny(@event, delay, cancel).ConfigureAwait(false);
		EventProvider.MessageReceived.Remove(Handler);

		if (task == cancel)
		{
			return InteractiveResult<T>.Canceled;
		}
		else if (task == delay)
		{
			return InteractiveResult<T>.TimedOut;
		}
		return await @event.ConfigureAwait(false);*/
	}

	/// <inheritdoc />
	protected Task<IUserMessage> ReplyAsync(
		string? message = null,
		bool isTTS = false,
		Embed? embed = null,
		RequestOptions? options = null,
		AllowedMentions? allowedMentions = null,
		Embed[]? embeds = null)
	{
		if (embed is not null)
		{
			if (embeds is null)
			{
				embeds = [embed];
			}
			else
			{
				Array.Resize(ref embeds, embeds.Length + 1);
				embeds[^1] = embed;
			}
		}

		return Context.Channel.SendMessageAsync(new SendMessageArgs
		{
			Content = message,
			IsTTS = isTTS,
			Embeds = embeds,
			Options = options,
			AllowedMentions = allowedMentions ?? AllowedMentions.None,
		});
	}
}