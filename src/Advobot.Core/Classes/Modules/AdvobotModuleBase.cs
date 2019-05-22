using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Advobot.Classes.Attributes.Preconditions;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Classes.Modules
{
	/// <summary>
	/// Shorter way to write the used modulebase and also has every command go through the <see cref="RequireCommandEnabledAttribute"/> first.
	/// </summary>
	[RequireCommandEnabled]
	[RequireContext(ContextType.Guild, Group = nameof(RequireContextAttribute))]
	public abstract class AdvobotModuleBase : ModuleBase<AdvobotCommandContext>
	{
		/// <summary>
		/// Attempts to parse a value from a message.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="message"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public delegate bool MessageTryParser<T>(SocketMessage message, out T value);

		/// <summary>
		/// How long timed messages should stay for.
		/// </summary>
		[DontInject]
		public TimeSpan MessageTime { get; set; } = TimeSpan.FromSeconds(5);
		/// <summary>
		/// The timers to use for deleting messages and other things.
		/// </summary>
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
		public ITimerService Timers { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
		/// <summary>
		/// The settings for the bot.
		/// </summary>
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
		public IBotSettings BotSettings { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.

		/// <summary>
		/// Uses user input to get the item at a specified index. This is blocking.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="format"></param>
		/// <returns></returns>
		public async Task<T> NextItemAtIndexAsync<T>(IList<T> source, Func<T, string> format)
		{
			var message = await ReplyAsync($"Did you mean any of the following:\n{source.FormatNumberedList(format)}").CAF();
			var index = await NextIndexAsync(0, source.Count - 1).CAF();
			await message.DeleteAsync(GenerateRequestOptions()).CAF();
#pragma warning disable CS8653 // A default expression introduces a null value for a type parameter.
			return index != null ? source[index.Value] : default;
#pragma warning restore CS8653 // A default expression introduces a null value for a type parameter.
		}
		/// <summary>
		/// Gets the next valid index supplied by the user. This is blocking.
		/// </summary>
		/// <param name="minVal"></param>
		/// <param name="maxVal"></param>
		/// <returns></returns>
		public async Task<int?> NextIndexAsync(int minVal, int maxVal)
		{
			return await NextValueAsync((SocketMessage x, out int? value) =>
			{
				value = null;
				var sameInvoker = x.Author.Id == Context.User.Id && x.Channel.Id == Context.Channel.Id;
				if (!sameInvoker || !int.TryParse(x.Content, out var position))
				{
					return false;
				}

				var index = position - 1;
				if (index >= minVal && index <= maxVal)
				{
					value = index;
					return true;
				}
				return false;
			});
		}
		/// <summary>
		/// Gets the next message which makes <paramref name="tryParser"/> return true. This is blocking.
		/// </summary>
		/// <param name="tryParser"></param>
		/// <returns></returns>
		/// <remarks>Heavily taken from https://github.com/foxbot/Discord.Addons.Interactive/blob/518d59227b5ede902f3d61fb8b07246fda017955/Discord.Addons.Interactive/InteractiveService.cs#L35</remarks>
		public async Task<T> NextValueAsync<T>(MessageTryParser<T> tryParser)
		{
			var eventTrigger = new TaskCompletionSource<T>();

			async Task Handler(SocketMessage message)
			{
				if (tryParser(message, out var value))
				{
					eventTrigger.SetResult(value);
					await message.DeleteAsync().CAF();
				}
			}

			Context.Client.MessageReceived += Handler;
			var trigger = eventTrigger.Task;
			var delay = Task.Delay(MessageTime);
			var task = await Task.WhenAny(trigger, delay).CAF();
			Context.Client.MessageReceived -= Handler;

#pragma warning disable CS8653 // A default expression introduces a null value for a type parameter.
			return task == trigger ? await trigger.CAF() : default;
#pragma warning restore CS8653 // A default expression introduces a null value for a type parameter.
		}
		/// <summary>
		/// Gets the main prefix to use in this module.
		/// </summary>
		/// <returns></returns>
		public string GetPrefix()
			=> BotSettings.GetPrefix(Context.GuildSettings);
		/// <summary>
		/// Gets a <see cref="RequestOptions"/> that mainly is used for the reason in the audit log.
		/// </summary>
		/// <param name="reason"></param>
		/// <returns></returns>
		public RequestOptions GenerateRequestOptions(string? reason = null)
			=> Context.GenerateRequestOptions(reason);
	}
}