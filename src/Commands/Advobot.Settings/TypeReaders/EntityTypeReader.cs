using Advobot.Modules;
using Advobot.Settings.Database.Models;
using Advobot.TypeReaders.Discord;

using Discord;

using MorseCode.ITask;

using YACCS.Results;
using YACCS.TypeReaders;

namespace Advobot.Settings.TypeReaders;

[TypeReaderTargetTypes(typeof(CommandOverrideEntity))]
public class EntityTypeReader : DiscordTypeReader<CommandOverrideEntity>
{
	private static readonly TextChannelTypeReader _ChannelTypeReader = new();
	private static readonly GuildTypeReader _GuildTypeReader = new();
	private static readonly RoleTypeReader _RoleTypeReader = new();
	private static readonly UserTypeReader _UserTypeReader = new();

	public override async ITask<ITypeReaderResult<CommandOverrideEntity>> ReadAsync(
		IGuildContext context,
		ReadOnlyMemory<string> input)
	{
		var userResult = await _UserTypeReader.ReadAsync(context, input).ConfigureAwait(false);
		if (userResult.InnerResult.IsSuccess && userResult.Value is IGuildUser user)
		{
			return Success(new(user));
		}

		var roleResult = await _RoleTypeReader.ReadAsync(context, input).ConfigureAwait(false);
		if (roleResult.InnerResult.IsSuccess && roleResult.Value is IRole role)
		{
			return Success(new(role));
		}

		var channelResult = await _ChannelTypeReader.ReadAsync(context, input).ConfigureAwait(false);
		if (channelResult.InnerResult.IsSuccess && channelResult.Value is ITextChannel channel)
		{
			return Success(new(channel));
		}

		var guildResult = await _GuildTypeReader.ReadAsync(context, input).ConfigureAwait(false);
		if (guildResult.InnerResult.IsSuccess && guildResult.Value is IGuild guild)
		{
			return Success(new(guild));
		}

		if (input.Span[0] == "guild")
		{
			return Success(new(context.Guild));
		}

		// TODO: singleton?
		return CachedResults<CommandOverrideEntity>.NotFound.Result;
	}
}