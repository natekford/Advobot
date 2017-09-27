using Advobot.Interfaces;
using Discord;

namespace Advobot.Modules.Log
{
	internal struct VerifiedLoggingAction
	{
		public IGuild Guild { get; }
		public IGuildSettings GuildSettings { get; }

		public VerifiedLoggingAction(IGuild guild, IGuildSettings guildSettings)
		{
			Guild = guild;
			GuildSettings = guildSettings;
		}
	}
}
