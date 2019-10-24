using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Advobot.Attributes.Preconditions;
using Advobot.Criterions;
using Advobot.Services.BotSettings;
using Advobot.Services.Timers;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

namespace Advobot.Modules
{
	/// <summary>
	/// Shorter way to write the used modulebase and also has every command go through the <see cref="RequireCommandEnabledAttribute"/> first.
	/// </summary>
	[RequireCommandEnabled]
	[RequireContext(ContextType.Guild, Group = nameof(RequireContextAttribute))]
	public abstract class AdvobotModuleBase : ModuleBase<AdvobotCommandContext>
	{
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
		/// The prefix for this context.
		/// </summary>
		public string Prefix => Context.Settings.GetPrefix(BotSettings);

		/// <summary>
		/// The timers to use for deleting messages and other things.
		/// </summary>
		public ITimerService Timers { get; set; } = null!;

		/// <summary>
		/// Gets a <see cref="RequestOptions"/> that mainly is used for the reason in the audit log.
		/// </summary>
		/// <param name="reason"></param>
		/// <returns></returns>
		public RequestOptions GenerateRequestOptions(string? reason = null)
			=> Context.GenerateRequestOptions(reason);

		/// <summary>
		/// Gets the next valid index supplied by the user. This is blocking.
		/// </summary>
		/// <param name="minVal"></param>
		/// <param name="maxVal"></param>
		/// <param name="timeout"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		public Task<int?> NextIndexAsync(
			int minVal,
			int maxVal,
			TimeSpan? timeout = null,
			CancellationToken token = default)
		{
			var criteria = new ICriterion<IMessage>[]
			{
				new EnsureSourceChannelCriterion(),
				new EnsureSourceUserCriterion(),
			};
			return NextValueAsync(criteria, (IMessage x) =>
			{
				if (!int.TryParse(x.Content, out var position))
				{
					return Task.FromResult<(bool, int?)>((false, null));
				}

				var index = position - 1;
				if (index >= minVal && index <= maxVal)
				{
					return Task.FromResult<(bool, int?)>((true, index));
				}
				return Task.FromResult<(bool, int?)>((false, null));
			}, timeout, token);
		}

		/// <summary>
		/// Uses user input to get the item at a specified index. This is blocking.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="format"></param>
		/// <param name="timeout"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		public async Task<T> NextItemAtIndexAsync<T>(
			IReadOnlyList<T> source,
			Func<T, string> format,
			TimeSpan? timeout = null,
			CancellationToken token = default)
		{
			var message = await ReplyAsync($"Did you mean any of the following:\n{source.FormatNumberedList(format)}").CAF();
			var index = await NextIndexAsync(0, source.Count - 1, timeout, token).CAF();
			await message.DeleteAsync(GenerateRequestOptions()).CAF();
			return index != null ? source[index.Value] : default;
		}

		/// <summary>
		/// Gets the next message which makes <paramref name="tryParser"/> return true. This is blocking.
		/// </summary>
		/// <param name="criteria"></param>
		/// <param name="tryParser"></param>
		/// <param name="timeout"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		/// <remarks>Heavily taken from https://github.com/foxbot/Discord.Addons.Interactive/blob/master/Discord.Addons.Interactive/InteractiveService.cs</remarks>
		public async Task<T> NextValueAsync<T>(
			IEnumerable<ICriterion<IMessage>> criteria,
			MessageTryParser<T> tryParser,
			TimeSpan? timeout = null,
			CancellationToken token = default)
		{
			timeout ??= DefaultInteractivityTime;

			var eventTrigger = new TaskCompletionSource<T>();
			var cancelTrigger = new TaskCompletionSource<bool>();
			token.Register(() => cancelTrigger.SetResult(true));

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

				var (success, value) = await tryParser(message).CAF();
				if (success)
				{
					eventTrigger.SetResult(value);
				}
			}

			Context.Client.MessageReceived += Handler;
			var trigger = eventTrigger.Task;
			var cancel = cancelTrigger.Task;
			var delay = Task.Delay(timeout.Value);
			var task = await Task.WhenAny(trigger, delay, cancel).CAF();
			Context.Client.MessageReceived -= Handler;

			return task == trigger ? await trigger.CAF() : default;
		}

		/// <summary>
		/// Attempts to parse a value from a message.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="message"></param>
		/// <returns></returns>
		public delegate Task<(bool, T)> MessageTryParser<T>(IMessage message);
	}
}