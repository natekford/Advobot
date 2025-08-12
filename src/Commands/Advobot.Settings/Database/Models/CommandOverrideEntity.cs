using Discord;

namespace Advobot.Settings.Database.Models;

public readonly struct CommandOverrideEntity
{
	public IEntity<ulong> Entity { get; }
	public CommandOverrideType EntityType { get; }
	public ulong GuildId { get; }

	public CommandOverrideEntity(IGuild guild)
	{
		GuildId = guild.Id;
		Entity = guild;
		EntityType = CommandOverrideType.Guild;
	}

	public CommandOverrideEntity(IRole role)
	{
		GuildId = role.Guild.Id;
		Entity = role;
		EntityType = CommandOverrideType.Role;
	}

	public CommandOverrideEntity(IGuildUser user)
	{
		GuildId = user.Guild.Id;
		Entity = user;
		EntityType = CommandOverrideType.User;
	}

	public CommandOverrideEntity(ITextChannel channel)
	{
		GuildId = channel.Guild.Id;
		Entity = channel;
		EntityType = CommandOverrideType.Channel;
	}
}