using System;
using System.Collections.Generic;
using Advobot.Classes;
using Advobot.Modules;
using Advobot.Utilities;
using Discord;
using static Advobot.Standard.Resources.Responses;

namespace Advobot.Standard.Responses
{
	public sealed class Guilds : CommandResponses
	{
		private Guilds() { }

		public static AdvobotResult LeftGuild(IGuild guild)
		{
			return Success(GuildsLeftGuild.Format(
				guild.Format().WithBlock()
			));
		}
		public static AdvobotResult DisplayRegions(IReadOnlyCollection<IVoiceRegion> regions)
		{
			var description = regions
				.ToDelimitedString(x => x.Id, Environment.NewLine)
				.WithBigBlock()
				.Value;
			return Success(new EmbedWrapper
			{
				Title = GuildsTitleRegionIds,
				Description = description,
			});
		}
		public static AdvobotResult ModifiedRegion(IVoiceRegion region)
		{
			return Success(GuildsModifiedRegion.Format(
				region.Id.WithBlock()
			));
		}
		public static AdvobotResult ModifiedAfkTime(int time)
		{
			return Success(GuildsModifiedAfkTime.Format(
				time.ToString().WithBlock()
			));
		}
		public static AdvobotResult ModifiedAfkChannel(IVoiceChannel? channel)
		{
			return Success(GuildsModifiedAfkChannel.Format(
				(channel?.Format() ?? GuildsVariableNothing).WithBlock()
			));
		}
		public static AdvobotResult ModifiedSystemChannel(ITextChannel? channel)
		{
			return Success(GuildsModifiedSystemChannel.Format(
				(channel?.Format() ?? GuildsVariableNothing).WithBlock()
			));
		}
		public static AdvobotResult ModifiedMsgNotif(DefaultMessageNotifications notifs)
		{
			return Success(GuildsModifiedMsgNotif.Format(
				notifs.ToString().WithBlock()
			));
		}
		public static AdvobotResult ModifiedVerif(VerificationLevel verif)
		{
			return Success(GuildsModifiedVerif.Format(
				verif.ToString().WithBlock()
			));
		}
		public static AdvobotResult EnqueuedSplash(int position)
		{
			return Success(GuildsEnqueuedSplash.Format(
				position.ToString().WithBlock()
			));
		}
		public static AdvobotResult RemovedSplash()
			=> Success(GuildsRemovedSplash);
		public static AdvobotResult ModifiedOwner(IUser user)
		{
			return Success(GuildsModifiedOwner.Format(
				user.Format().WithBlock()
			));
		}
	}
}
