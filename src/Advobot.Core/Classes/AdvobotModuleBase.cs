using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes.Attributes;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;

namespace Advobot.Classes
{
	/// <summary>
	/// Shorter way to write the used modulebase and also has every command go through the <see cref="CommandRequirementAttribute"/> first.
	/// </summary>
	[CommandRequirement]
	[RequireContext(ContextType.Guild)]
	public abstract class AdvobotModuleBase : ModuleBase<AdvobotCommandContext>
	{
		/// <summary>
		/// How long timed messages should stay for.
		/// </summary>
		[DontInject]
		public TimeSpan MessageTime { get; set; } = TimeSpan.FromSeconds(3);
		/// <summary>
		/// The timers to use for deleting messages and other things.
		/// </summary>
		public ITimerService Timers { get; set; }
		/// <summary>
		/// The settings fo the bot.
		/// </summary>
		public IBotSettings BotSettings { get; set; }

		/// <summary>
		/// Sends a short message saying there are none of the objects or sends an embed describing each object.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="title"></param>
		/// <param name="func"></param>
		/// <returns></returns>
		public async Task<IUserMessage> ReplyIfAny<T>(IEnumerable<T> source, string title, Func<T, string> func)
			=> await ReplyIfAny(source, null, title, func).CAF();
		/// <summary>
		/// Sends a short message saying there are none of the objects or sends an embed describing each object.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="target"></param>
		/// <param name="title"></param>
		/// <param name="func"></param>
		/// <returns></returns>
		public async Task<IUserMessage> ReplyIfAny<T>(IEnumerable<T> source, object target, string title, Func<T, string> func)
		{
			var targetStr = target == null ? "" : $" for `{target.Format()}`";
			if (!source.Any())
			{
				return await ReplyTimedAsync($"There are zero {title.ToLower()}{targetStr}.").CAF();
			}
			return await ReplyEmbedAsync(new EmbedWrapper
			{
				Title = $"{title.FormatTitle()}{targetStr}",
				Description = source.FormatNumberedList(func),
			}).CAF();
		}
		/// <summary>
		/// Sends a custom message if there are none of the objects or sends an even more custom message if there are.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="response"></param>
		/// <param name="task"></param>
		/// <returns></returns>
		public async Task<IUserMessage> ReplyIfAny<T>(IEnumerable<T> source, string response, Func<IEnumerable<T>, Task<IUserMessage>> task)
			=> await (source.Any() ? task.Invoke(source) : ReplyTimedAsync(response)).CAF();
		/// <summary>
		/// Sends a message containing text and optionally an embed or textfile.
		/// </summary>
		/// <param name="content"></param>
		/// <param name="wrapper"></param>
		/// <param name="textFile"></param>
		/// <returns></returns>
		public async Task<IUserMessage> ReplyAsync(string content, EmbedWrapper wrapper = null, TextFileInfo textFile = null)
			=> await MessageUtils.SendMessageAsync(Context.Channel, content, wrapper, textFile).CAF();
		/// <summary>
		/// Sends a message containing an embed and some text.
		/// </summary>
		/// <param name="content"></param>
		/// <param name="wrapper"></param>
		/// <returns></returns>
		public async Task<IUserMessage> ReplyEmbedAsync(string content, EmbedWrapper wrapper)
			=> await ReplyAsync(content, wrapper, null).CAF();
		/// <summary>
		/// Sends a message containing only an embed.
		/// </summary>
		/// <param name="wrapper"></param>
		/// <returns></returns>
		public async Task<IUserMessage> ReplyEmbedAsync(EmbedWrapper wrapper)
			=> await ReplyAsync(null, wrapper, null).CAF();
		/// <summary>
		/// Sends a message containing a file and some text.
		/// </summary>
		/// <param name="content"></param>
		/// <param name="textFile"></param>
		/// <returns></returns>
		public async Task<IUserMessage> ReplyFileAsync(string content, TextFileInfo textFile)
			=> await ReplyAsync(content, null, textFile).CAF();
		/// <summary>
		/// Sends a message containing only a file.
		/// </summary>
		/// <param name="textFile"></param>
		/// <returns></returns>
		public async Task<IUserMessage> ReplyFileAsync(TextFileInfo textFile)
			=> await ReplyAsync(null, null, textFile).CAF();
		/// <summary>
		/// Send an error message which will be deleted after some time unless the guild settings have errors disabled.
		/// </summary>
		/// <param name="error"></param>
		/// <returns></returns>
		public async Task<IUserMessage> ReplyErrorAsync(Error error)
			=> Context.GuildSettings.NonVerboseErrors ? null : await ReplyTimedAsync($"**ERROR:** {error.Reason}").CAF();
		/// <summary>
		/// Sends a message which gets deleted after some time.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public async Task<IUserMessage> ReplyTimedAsync(string output)
		{
			var secondMessage = await ReplyAsync(output).CAF();
			var removableMessage = new RemovableMessage(Context, new[] { secondMessage }, MessageTime);
			if (Timers != null)
			{
				await Timers.AddAsync(removableMessage).CAF();
			}
			return secondMessage;
		}
		/// <summary>
		/// Gets the main prefix to use in this module.
		/// </summary>
		/// <returns></returns>
		public string GetPrefix()
			=> BotSettings.InternalGetPrefix(Context.GuildSettings);
		/// <summary>
		/// Gets a request options that mainly is used for the reason in the audit log.
		/// </summary>
		/// <param name="reason"></param>
		/// <returns></returns>
		public RequestOptions GetRequestOptions(string reason = "")
		{
			var r = string.IsNullOrWhiteSpace(reason) ? "" : $" Reason: {reason}.";
			return ClientUtils.CreateRequestOptions($"Action by {Context.User.Format()}.{r}");
		}
	}
}