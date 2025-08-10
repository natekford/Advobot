using Advobot.Utilities;

using Discord;
using Discord.Commands;

namespace Advobot.TypeReaders;

/// <summary>
/// Attempts to find an <see cref="IBan"/> on a guild.
/// </summary>
[TypeReaderTargetType(typeof(IBan))]
public sealed class BanTypeReader : TypeReader
{
	/// <summary>
	/// Checks for any bans matching the input. Input is tested as a user id, username and discriminator, and finally solely the username.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="input"></param>
	/// <param name="services"></param>
	/// <returns></returns>
	public override async Task<TypeReaderResult> ReadAsync(
		ICommandContext context,
		string input,
		IServiceProvider services)
	{
		var ban = default(IBan?);
		if (MentionUtils.TryParseUser(input, out var id) || ulong.TryParse(input, out id))
		{
			ban = await context.Guild.GetBanAsync(id).ConfigureAwait(false);
		}

		if (ban is not null)
		{
			return TypeReaderResult.FromSuccess(ban);
		}
		return TypeReaderUtils.ParseFailedResult<IBan>();
	}
}