using Advobot.Utilities;

using Discord;
using Discord.Commands;

namespace Advobot.TypeReaders;

/// <summary>
/// Attempts to find an <see cref="IInviteMetadata"/> on the guild.
/// </summary>
[TypeReaderTargetType(typeof(IInviteMetadata))]
public sealed class InviteTypeReader : TypeReader
{
	/// <inheritdoc />
	public override async Task<TypeReaderResult> ReadAsync(
		ICommandContext context,
		string input,
		IServiceProvider services)
	{
		var code = input.Split('/')[^1];
		var invite = await context.Client.GetInviteAsync(code).ConfigureAwait(false);
		if (invite is not null && invite.GuildId == context.Guild.Id)
		{
			var channel = await context.Guild.GetChannelAsync(invite.ChannelId).ConfigureAwait(false);
			if (channel is INestedChannel nestedChannel)
			{
				var invites = await nestedChannel.GetInvitesAsync().ConfigureAwait(false);
				return TypeReaderResult.FromSuccess(invites.Single(x => x.Id == invite.Id));
			}
		}
		return TypeReaderUtils.ParseFailedResult<IInviteMetadata>();
	}
}