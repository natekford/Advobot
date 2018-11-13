using System;
using System.Collections.Generic;
using System.Linq;
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
	/// Shorter way to write the used modulebase and also has every command go through the <see cref="CommandEnabledAttribute"/> first.
	/// </summary>
	[CommandEnabled]
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
		public ITimerService Timers { get; set; }
		/// <summary>
		/// The settings for the bot.
		/// </summary>
		public IBotSettings BotSettings { get; set; }

		/// <summary>
		/// Sends a short message saying there are none of the objects or sends an embed describing each object.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source">The enumerable to check for items.</param>
		/// <param name="title">The type of objects.</param>
		/// <param name="some">What to use when there are any elements in <paramref name="source"/>.</param>
		/// <returns></returns>
		public Task<IUserMessage> ReplyIfAny<T>(IEnumerable<T> source, string title, Func<T, string> some)
			=> ReplyIfAny(source, null, title, some);
		/// <summary>
		/// Sends a short message saying there are none of the objects or sends an embed describing each object.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source">The enumerable to check for items.</param>
		/// <param name="target">The object that these items are children to.</param>
		/// <param name="title">The type of objects.</param>
		/// <param name="some">What to use when there are any elements in <paramref name="source"/>.</param>
		/// <returns></returns>
		public async Task<IUserMessage> ReplyIfAny<T>(IEnumerable<T> source, object target, string title, Func<T, string> some)
		{
			var targetStr = target == null ? "" : $" for `{target.Format()}`";
			if (!source.Any())
			{
				return await ReplyTimedAsync($"There are zero {title.ToLower()}{targetStr}.").CAF();
			}
			return await ReplyEmbedAsync(new EmbedWrapper
			{
				Title = $"{title.FormatTitle()}{targetStr}",
				Description = source.FormatNumberedList(some),
			}).CAF();
		}
		/// <summary>
		/// Responds with <paramref name="none"/> if there are zero elements, otherwise responds with <paramref name="some"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source">The enumerable to check for items.</param>
		/// <param name="none">What to use when there are zero elements in <paramref name="source"/>.</param>
		/// <param name="some">What to use when there are any elements in <paramref name="source"/>.</param>
		/// <returns></returns>
		public Task<IUserMessage> ReplyIfAny<T>(IEnumerable<T> source, string none, string some)
			=> source.Any() ? ReplyAsync(some) : ReplyTimedAsync(none);
		/// <summary>
		/// Sends a custom message if there are none of the objects or sends an even more custom message if there are.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source">The enumerable to check for items.</param>
		/// <param name="none">What to use when there are zero elements in <paramref name="source"/>.</param>
		/// <param name="some">What to use when there are any elements in <paramref name="source"/>.</param>
		/// <returns></returns>
		public Task<IUserMessage> ReplyIfAny<T>(IEnumerable<T> source, string none, Func<IEnumerable<T>, Task<IUserMessage>> some)
			=> source.Any() ? some.Invoke(source) : ReplyTimedAsync(none);
		/// <summary>
		/// Sends a message containing text and optionally an embed or textfile.
		/// </summary>
		/// <param name="content"></param>
		/// <param name="wrapper"></param>
		/// <param name="textFile"></param>
		/// <returns></returns>
		public Task<IUserMessage> ReplyAsync(string content, EmbedWrapper wrapper = null, TextFileInfo textFile = null)
			=> MessageUtils.SendMessageAsync(Context.Channel, content, wrapper, textFile);
		/// <summary>
		/// Sends a message containing an embed and some text.
		/// </summary>
		/// <param name="content"></param>
		/// <param name="wrapper"></param>
		/// <returns></returns>
		public Task<IUserMessage> ReplyEmbedAsync(string content, EmbedWrapper wrapper)
			=> ReplyAsync(content, wrapper, null);
		/// <summary>
		/// Sends a message containing only an embed.
		/// </summary>
		/// <param name="wrapper"></param>
		/// <returns></returns>
		public Task<IUserMessage> ReplyEmbedAsync(EmbedWrapper wrapper)
			=> ReplyAsync(null, wrapper, null);
		/// <summary>
		/// Sends a message containing a file and some text.
		/// </summary>
		/// <param name="content"></param>
		/// <param name="textFile"></param>
		/// <returns></returns>
		public Task<IUserMessage> ReplyFileAsync(string content, TextFileInfo textFile)
			=> ReplyAsync(content, null, textFile);
		/// <summary>
		/// Sends a message containing only a file.
		/// </summary>
		/// <param name="textFile"></param>
		/// <returns></returns>
		public Task<IUserMessage> ReplyFileAsync(TextFileInfo textFile)
			=> ReplyAsync(null, null, textFile);
		/// <summary>
		/// Send an error message which will be deleted after some time unless the guild settings have errors disabled.
		/// </summary>
		/// <param name="reason"></param>
		/// <returns></returns>
		public Task<IUserMessage> ReplyErrorAsync(string reason)
			=> Context.GuildSettings.NonVerboseErrors ? Task.FromResult<IUserMessage>(default) : ReplyTimedAsync($"**ERROR:** {reason}");
		/// <summary>
		/// Sends a message which gets deleted after some time.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public async Task<IUserMessage> ReplyTimedAsync(string output)
		{
			var secondMessage = await ReplyAsync(output).CAF();
			var removableMessage = new RemovableMessage(Context, new[] { secondMessage }, MessageTime);
			await Timers.AddAsync(removableMessage).CAF();
			return secondMessage;
		}
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
			return index != null ? source[index.Value] : default;
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

			return task == trigger ? await trigger.CAF() : default;
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
		public RequestOptions GenerateRequestOptions(string reason = null)
			=> Context.GenerateRequestOptions(reason);
	}
}