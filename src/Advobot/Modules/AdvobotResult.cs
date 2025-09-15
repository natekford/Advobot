using Advobot.Embeds;
using Advobot.Utilities;

using Discord;

using YACCS.Results;

namespace Advobot.Modules;

/// <summary>
/// A result which should only be logged once.
/// </summary>
public class AdvobotResult : IResult
{
	/// <summary>
	/// The embed to post with the message.
	/// </summary>
	public EmbedWrapper? Embed { get; set; }
	/// <summary>
	/// The file to post with the message.
	/// </summary>
	public FileAttachment? File { get; set; }
	/// <inheritdoc />
	public bool IsSuccess { get; set; }
	/// <summary>
	/// Where to send this result to. If this is null, the default context channel will be used instead.
	/// </summary>
	public ulong? OverrideDestinationChannelId { get; set; }
	/// <inheritdoc />
	public string Response { get; set; } = "";
	/// <summary>
	/// How long to let this message stay up for.
	/// </summary>
	public TimeSpan? Time { get; set; }

	/// <summary>
	/// Creates an error result.
	/// </summary>
	/// <param name="reason"></param>
	/// <returns></returns>
	public static AdvobotResult Failure(string reason)
	{
		return new()
		{
			IsSuccess = false,
			Response = reason,
		};
	}

	/// <summary>
	/// Converts the result into a task returning the result.
	/// </summary>
	/// <param name="result"></param>
	public static implicit operator Task<AdvobotResult>(AdvobotResult result)
		=> Task.FromResult(result);

	/// <summary>
	/// Creates a successful result.
	/// </summary>
	/// <param name="reason"></param>
	/// <returns></returns>
	public static AdvobotResult Success(string reason)
	{
		if (reason.Length < 2000)
		{
			return new()
			{
				IsSuccess = true,
				Response = reason,
			};
		}
		return Success(MessageUtils.CreateTextFile("Message_Too_Long", reason));
	}

	/// <summary>
	/// Creates a successful result.
	/// </summary>
	/// <param name="embed"></param>
	/// <returns></returns>
	public static AdvobotResult Success(EmbedWrapper embed)
	{
		var response = Success(Constants.ZERO_WIDTH_SPACE);
		response.Embed = embed;
		return response;
	}

	/// <summary>
	/// Creates a successful result.
	/// </summary>
	/// <param name="file"></param>
	/// <returns></returns>
	public static AdvobotResult Success(FileAttachment file)
	{
		var response = Success(Constants.ZERO_WIDTH_SPACE);
		response.File = file;
		return response;
	}

	/// <summary>
	/// Sends this result to the specified context.
	/// </summary>
	/// <param name="context"></param>
	/// <returns></returns>
	public async Task<IUserMessage> SendAsync(IGuildContext context)
	{
		var destination = context.Channel;
		if (OverrideDestinationChannelId is ulong id)
		{
			destination = await context.Guild.GetTextChannelAsync(id).ConfigureAwait(false);
			if (destination is null)
			{
				return await context.Channel.SendMessageAsync(new SendMessageArgs
				{
					Content = $"{id} is not a valid destination channel.",
				}).ConfigureAwait(false);
			}
		}

		return await destination.SendMessageAsync(new SendMessageArgs(Embed)
		{
			Content = Response,
			Files = File.HasValue ? [File.Value] : null,
		}).ConfigureAwait(false);
	}

	/// <summary>
	/// Returns the reason of this result.
	/// </summary>
	/// <returns></returns>
	public override string ToString()
		=> Response;
}