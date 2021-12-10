using Advobot.SQLite.Relationships;

using Discord;

namespace Advobot.Settings.Models;

public sealed record CommandOverride(
	string CommandId,
	bool Enabled,
	ulong GuildId,
	int Priority,
	ulong TargetId,
	CommandOverrideType TargetType
) : IGuildChild
{
	public CommandOverride() : this("", default, default, default, default, default) { }

	public CommandOverride(ulong guildId, ulong targetId, CommandOverrideType targetType)
		: this("", default, guildId, default, targetId, targetType) { }

	public CommandOverride(CommandOverrideEntity entity)
		: this(entity.GuildId, entity.Entity.Id, entity.EntityType) { }

	public CommandOverride(IGuild guild)
		: this(guild.Id, guild.Id, CommandOverrideType.Guild) { }

	public CommandOverride(IRole role)
		: this(role.Guild.Id, role.Id, CommandOverrideType.Role) { }

	public CommandOverride(IGuildUser user)
		: this(user.Guild.Id, user.Id, CommandOverrideType.User) { }

	public CommandOverride(ITextChannel channel)
		: this(channel.Guild.Id, channel.Id, CommandOverrideType.Channel) { }
}