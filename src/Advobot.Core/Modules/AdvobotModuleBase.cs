﻿using Advobot.Interactivity;
using Advobot.Interactivity.Criterions;
using Advobot.Interactivity.TryParsers;
using Advobot.Preconditions;
using Advobot.Punishments;
using Advobot.Services.BotSettings;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

namespace Advobot.Modules;

/// <summary>
/// Shorter way to write the used modulebase and also has every command go through the <see cref="ExtendableCommandValidation"/> first.
/// </summary>
[ExtendableCommandValidation]
[RequireContext(ContextType.Guild, Group = nameof(RequireContextAttribute))]
public abstract class AdvobotModuleBase : ModuleBase<AdvobotCommandContext>
{
	private static readonly ICriterion<IMessage>[] _NextIndexCriteria = new ICriterion<IMessage>[]
	{
		new EnsureSourceChannelCriterion(),
		new EnsureSourceUserCriterion(),
	};

	/// <summary>
	/// The settings for the bot.
	/// </summary>
	public IBotSettings BotSettings { get; set; } = null!;
	/// <summary>
	/// The default time to wait for a user's response.
	/// </summary>
	[DontInject]
	public TimeSpan DefaultInteractivityTime { get; set; } = TimeSpan.FromSeconds(5);
	/// <summary>
	/// The punisher to use for giving punishments.
	/// </summary>
	public IPunisher Punisher { get; set; } = null!;

	/// <summary>
	/// Gets a <see cref="RequestOptions"/> that mainly is used for the reason in the audit log.
	/// </summary>
	/// <param name="reason"></param>
	/// <returns></returns>
	public RequestOptions GetOptions(string? reason = null)
		=> Context.GenerateRequestOptions(reason);

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
		var message = await ReplyAsync($"Did you mean any of the following:\n{source.FormatNumberedList(format)}").CAF();
		var index = await NextIndexAsync(0, source.Count - 1, options).CAF();
		await message.DeleteAsync(GetOptions()).CAF();
		return index.HasValue ? source[index.Value] : InteractiveResult<T>.PropagateError(index);
	}

	/// <summary>
	/// Gets the next message which makes <paramref name="tryParser"/> return true. This is blocking.
	/// </summary>
	/// <param name="tryParser"></param>
	/// <param name="options"></param>
	/// <returns></returns>
	/// <remarks>Heavily taken from https://github.com/foxbot/Discord.Addons.Interactive/blob/master/Discord.Addons.Interactive/InteractiveService.cs</remarks>
	public async Task<InteractiveResult<T>> NextValueAsync<T>(
		IMessageTryParser<T> tryParser,
		InteractivityOptions? options = null)
	{
		var timeout = options?.Timeout ?? DefaultInteractivityTime;

		var eventTrigger = new TaskCompletionSource<T>();
		var cancelTrigger = new TaskCompletionSource<bool>();
		if (options?.Token is CancellationToken token)
		{
			token.Register(() => cancelTrigger.SetResult(true));
		}

		var criteria = options?.Criteria ?? Array.Empty<ICriterion<IMessage>>();
		async Task Handler(IMessage message)
		{
			foreach (var criterion in criteria)
			{
				var result = await criterion.JudgeAsync(Context, message).CAF();
				if (!result)
				{
					return;
				}
			}

			var parsed = await tryParser.TryParseAsync(message).CAF();
			if (parsed.IsSpecified)
			{
				eventTrigger.SetResult(parsed.Value);
			}
		}

		Context.Client.MessageReceived += Handler;
		var @event = eventTrigger.Task;
		var cancel = cancelTrigger.Task;
		var delay = Task.Delay(timeout);
		var task = await Task.WhenAny(@event, delay, cancel).CAF();
		Context.Client.MessageReceived -= Handler;

		if (task == cancel)
		{
			return InteractiveResult<T>.Canceled;
		}
		else if (task == delay)
		{
			return InteractiveResult<T>.TimedOut;
		}
		return await @event.CAF();
	}

	/// <inheritdoc />
	protected override Task<IUserMessage> ReplyAsync(
		string? message = null,
		bool isTTS = false,
		Embed? embed = null,
		RequestOptions? options = null,
		AllowedMentions? allowedMentions = null,
		MessageReference? messageReference = null,
		MessageComponent? component = null,
		ISticker[]? stickers = null,
		Embed[]? embeds = null)
	{
		if (embed is not null)
		{
			if (embeds is null)
			{
				embeds = new[] { embed };
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
			AllowedMentions = allowedMentions,
		});
	}
}