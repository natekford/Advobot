using Advobot.Settings.ReadOnlyModels;

using Discord;

namespace Advobot.Settings.Models
{
	public sealed class CommandOverride : IReadOnlyCommandOverride
	{
		public string CommandId { get; set; }
		public bool Enabled { get; set; }
		public ulong GuildId { get; set; }
		public int Priority { get; set; }
		public ulong TargetId { get; set; }
		public CommandOverrideType TargetType { get; set; }

		public CommandOverride()
		{
			CommandId = "";
		}

		public CommandOverride(CommandOverrideEntity entity) : this()
		{
			GuildId = entity.GuildId;
			TargetId = entity.Entity.Id;
			TargetType = entity.EntityType;
		}

		public CommandOverride(IGuild guild) : this()
		{
			GuildId = guild.Id;
			TargetId = guild.Id;
			TargetType = CommandOverrideType.Guild;
		}

		public CommandOverride(IRole role) : this()
		{
			GuildId = role.Guild.Id;
			TargetId = role.Id;
			TargetType = CommandOverrideType.Role;
		}

		public CommandOverride(IGuildUser user) : this()
		{
			GuildId = user.Guild.Id;
			TargetId = user.Id;
			TargetType = CommandOverrideType.User;
		}

		public CommandOverride(ITextChannel channel) : this()
		{
			GuildId = channel.Guild.Id;
			TargetId = channel.Id;
			TargetType = CommandOverrideType.Channel;
		}
	}
}