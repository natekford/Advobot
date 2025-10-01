using Discord;

namespace Advobot.Settings.Database.Models;

public readonly record struct CommandOverrideEntity(
	IEntity<ulong> Entity,
	CommandOverrideType EntityType,
	ulong GuildId
)
{
	public CommandOverrideEntity(IGuild guild)
		: this(guild, CommandOverrideType.Guild, guild.Id)
	{
	}

	public CommandOverrideEntity(IRole role)
		: this(role, CommandOverrideType.Role, role.Guild.Id)
	{
	}

	public CommandOverrideEntity(IGuildUser user)
		: this(user, CommandOverrideType.User, user.Guild.Id)
	{
	}

	public CommandOverrideEntity(ITextChannel channel)
		: this(channel, CommandOverrideType.Channel, channel.Guild.Id)
	{
	}
}