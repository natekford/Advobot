using Advobot.Classes.Results;
using Advobot.Classes.Settings;

namespace Advobot.Commands.Responses
{
	public sealed class GuildSettings : CommandResponses
	{
		private GuildSettings() { }

		public static AdvobotResult SendWelcomeNotification(GuildNotification? notif)
			=> SendNotification(notif, "welcome");
		public static AdvobotResult SendGoodbyeNotification(GuildNotification? notif)
			=> SendNotification(notif, "goodbye");

		private static AdvobotResult SendNotification(GuildNotification? notif, string notifName)
		{
			if (notif == null)
			{
				return Failure($"The {notifName} notification does not exist.").WithTime(DefaultTime);
			}
			return Success(notif.Content ?? Constants.ZERO_LENGTH_CHAR)
				.WithEmbed(notif.CustomEmbed?.BuildWrapper())
				.WithOverrideDestinationChannelId(notif.ChannelId);
		}
	}
}
