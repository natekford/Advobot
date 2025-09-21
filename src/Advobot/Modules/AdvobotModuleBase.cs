using Advobot.Interactivity;
using Advobot.ParameterPreconditions.Numbers;
using Advobot.Preconditions;
using Advobot.Services.BotConfig;
using Advobot.Services.Events;
using Advobot.Services.Punishments;
using Advobot.Utilities;

using Discord;

using YACCS.Commands;
using YACCS.Commands.Building;
using YACCS.Interactivity;
using YACCS.TypeReaders;

namespace Advobot.Modules;

/// <summary>
/// Shorter way to write the used modulebase and also has every command go through the <see cref="ExtendableCommandValidation"/> first.
/// </summary>
[ExtendableCommandValidation]
public abstract class AdvobotModuleBase : CommandGroup<IGuildContext>
{
	private static readonly ICriterion<IGuildContext, IMessage>[] _NextIndexCriteria =
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
	/// The service to use for getting additional input from users.
	/// </summary>
	[InjectService]
	public required DiscordMessageInput MessageInput { get; set; }
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
	/// Gets the next valid index supplied by the invoker.
	/// </summary>
	/// <param name="min"></param>
	/// <param name="max"></param>
	/// <returns></returns>
	public Task<ITypeReaderResult<int>> NextIndexAsync(int min, int max)
	{
		return MessageInput.GetAsync<int>(Context, new()
		{
			Criteria = _NextIndexCriteria,
			Preconditions = [new NumberParameterPrecondition(min, max)],
		});
	}

	/// <summary>
	/// Processes invoker message input to get the item at a specified index.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="source"></param>
	/// <param name="format"></param>
	/// <returns></returns>
	public async Task<ITypeReaderResult<T>> NextItemAtIndexAsync<T>(
		IReadOnlyList<T> source,
		Func<T, string> format)
	{
		var list = source.Select(format).FormatNumberedList();
		var message = await ReplyAsync($"Did you mean any of the following:\n{list}").ConfigureAwait(false);
		var index = await NextIndexAsync(1, source.Count).ConfigureAwait(false);
		await message.DeleteAsync(GetOptions()).ConfigureAwait(false);

		if (index.InnerResult.IsSuccess)
		{
			return TypeReaderResult<T>.Success(source[index.Value - 1]);
		}
		else
		{
			return TypeReaderResult<T>.Error(index.InnerResult);
		}
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