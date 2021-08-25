
using Advobot.Attributes;
using Advobot.Settings.Models;
using Advobot.TypeReaders;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

namespace Advobot.Settings.TypeReaders
{
	[TypeReaderTargetType(typeof(CommandOverrideEntity))]
	public class EntityTypeReader : TypeReader
	{
		private readonly static ChannelTypeReader<ITextChannel> _ChannelTypeReader = new();
		private readonly static GuildTypeReader _GuildTypeReader = new();
		private readonly static RoleTypeReader<IRole> _RoleTypeReader = new();
		private readonly static UserTypeReader<IGuildUser> _UserTypeReader = new();

		public override async Task<TypeReaderResult> ReadAsync(
			ICommandContext context,
			string input,
			IServiceProvider services)
		{
			var userResult = await _UserTypeReader.ReadAsync(context, input, services).CAF();
			if (userResult.IsSuccess && userResult.BestMatch is IGuildUser user)
			{
				return TypeReaderResult.FromSuccess(new CommandOverrideEntity(user));
			}

			var roleResult = await _RoleTypeReader.ReadAsync(context, input, services).CAF();
			if (roleResult.IsSuccess && roleResult.BestMatch is IRole role)
			{
				return TypeReaderResult.FromSuccess(new CommandOverrideEntity(role));
			}

			var channelResult = await _ChannelTypeReader.ReadAsync(context, input, services).CAF();
			if (channelResult.IsSuccess && channelResult.BestMatch is ITextChannel channel)
			{
				return TypeReaderResult.FromSuccess(new CommandOverrideEntity(channel));
			}

			var guildResult = await _GuildTypeReader.ReadAsync(context, input, services).CAF();
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
}