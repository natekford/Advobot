using Advobot.Settings.Database.Models;
using Advobot.TypeReaders;
using Advobot.Utilities;

using Discord;
using Discord.Commands;

namespace Advobot.Settings.TypeReaders;

[TypeReaderTargetType(typeof(CommandOverrideEntity))]
public class EntityTypeReader : TypeReader
{
	private static readonly ChannelTypeReader<ITextChannel> _ChannelTypeReader = new();
	private static readonly GuildTypeReader _GuildTypeReader = new();
	private static readonly RoleTypeReader<IRole> _RoleTypeReader = new();
	private static readonly UserTypeReader<IGuildUser> _UserTypeReader = new();

	public override async Task<TypeReaderResult> ReadAsync(
		ICommandContext context,
		string input,
		IServiceProvider services)
	{
		var userResult = await _UserTypeReader.ReadAsync(context, input, services).ConfigureAwait(false);
		if (userResult.IsSuccess && userResult.BestMatch is IGuildUser user)
		{
			return TypeReaderResult.FromSuccess(new CommandOverrideEntity(user));
		}

		var roleResult = await _RoleTypeReader.ReadAsync(context, input, services).ConfigureAwait(false);
		if (roleResult.IsSuccess && roleResult.BestMatch is IRole role)
		{
			return TypeReaderResult.FromSuccess(new CommandOverrideEntity(role));
		}

		var channelResult = await _ChannelTypeReader.ReadAsync(context, input, services).ConfigureAwait(false);
		if (channelResult.IsSuccess && channelResult.BestMatch is ITextChannel channel)
		{
			return TypeReaderResult.FromSuccess(new CommandOverrideEntity(channel));
		}

		var guildResult = await _GuildTypeReader.ReadAsync(context, input, services).ConfigureAwait(false);
		if (guildResult.IsSuccess && guildResult.BestMatch is IGuild guild)
		{
			return TypeReaderResult.FromSuccess(new CommandOverrideEntity(guild));
		}

		if (input.CaseInsEquals("guild"))
		{
			return TypeReaderResult.FromSuccess(new CommandOverrideEntity(context.Guild));
		}

		return TypeReaderResult.FromError(CommandError.ObjectNotFound, "Unable to find a targetable entity.");
	}
}