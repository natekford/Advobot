using System.Collections.Generic;
using System.Linq;
using Advobot.Classes;
using Advobot.Classes.Results;
using Advobot.Utilities;
using Discord;

namespace Advobot.Commands.Responses
{
	public sealed class Guilds : CommandResponses
	{
		private Guilds() { }

		public static AdvobotResult NotBotOwner()
			=> Failure(Default.FormatInterpolated($"Only the bot owner can use this command targetting other guilds."));
		public static AdvobotResult InvalidGuild(ulong guildId)
			=> Failure(Default.FormatInterpolated($"Failed to find a guild with the id {guildId}.")).WithTime(DefaultTime);
		public static AdvobotResult LeftGuild(IGuild guild)
			=> Success(Default.FormatInterpolated($"Successfully left {guild}."));
		public static AdvobotResult DisplayRegions(IReadOnlyCollection<IVoiceRegion> regions)
		{
			return Success(new EmbedWrapper
			{
				Title = "Region Ids",
				Description = Default.FormatInterpolated($"{regions.Select(x => x.Id)}"),
			});
		}
		public static AdvobotResult ModifiedRegion(IVoiceRegion region)
			=> Success(Default.FormatInterpolated($"Successfully changed the server region to {region.Id}."));
		public static AdvobotResult ModifiedAfkTime(int time)
			=> Success(Default.FormatInterpolated($"Successfully changed the AFK timeout to {time} minutes."));
		public static AdvobotResult ModifiedAfkChannel(IVoiceChannel? channel)
			=> Success(Default.FormatInterpolated($"Successfully changed the AFK channel to {channel}."));
		public static AdvobotResult ModifiedSystemChannel(ITextChannel? channel)
			=> Success(Default.FormatInterpolated($"Successfully changed the system channel to {channel}."));
		public static AdvobotResult ModifiedMsgNotif(DefaultMessageNotifications notifs)
			=> Success(Default.FormatInterpolated($"Successfully changed the default message notification setting to {notifs}."));
		public static AdvobotResult ModifiedVerif(VerificationLevel verif)
			=> Success(Default.FormatInterpolated($"Successfully changed the verification level to {verif}."));
		public static AdvobotResult EnqueuedSplash(int position)
			=> Success(Default.FormatInterpolated($"Successfully queued changing the splash image at position {position}."));
		public static AdvobotResult RemovedSplash()
			=> Success("Successfully removed the splash image.");
		public static AdvobotResult ModifiedOwner(IUser user)
			=> Success(Default.FormatInterpolated($"Successfully changed the owner to {user}."));
	}
}
