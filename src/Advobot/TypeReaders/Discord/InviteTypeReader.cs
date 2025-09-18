using Advobot.Modules;

using Discord;

using MorseCode.ITask;

using YACCS.TypeReaders;

namespace Advobot.TypeReaders.Discord;

/// <summary>
/// Attempts to find an <see cref="IInviteMetadata"/>.
/// </summary>
[TypeReaderTargetTypes(typeof(IInviteMetadata))]
public sealed class InviteTypeReader : DiscordTypeReader<IInviteMetadata>
{
	/// <inheritdoc />
	public override async ITask<ITypeReaderResult<IInviteMetadata>> ReadAsync(
		IGuildContext context,
		ReadOnlyMemory<string> input)
	{
		var joined = Join(context, input);
		var code = joined.Split('/')[^1];
		var invite = await context.Client.GetInviteAsync(code).ConfigureAwait(false);
		if (invite is not null && invite.GuildId == context.Guild.Id)
		{
			var channel = await context.Guild.GetChannelAsync(invite.ChannelId).ConfigureAwait(false);
			if (channel is INestedChannel nestedChannel)
			{
				var invites = await nestedChannel.GetInvitesAsync().ConfigureAwait(false);
				return Success(invites.Single(x => x.Id == invite.Id));
			}
		}

		return TypeReaderResult<IInviteMetadata>.ParseFailed.Result;
	}
}