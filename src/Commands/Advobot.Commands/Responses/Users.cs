using Advobot.Classes;
using Advobot.Classes.Formatting;
using Advobot.Classes.Results;
using Advobot.Utilities;
using Discord;
using System.Collections.Generic;
using System.Linq;

namespace Advobot.CommandMarking.Responses
{
	public sealed class Users : CommandResponses
	{
		private Users() { }

		public static AdvobotResult Muted(bool punished, IUser user, PunishmentArgs args)
			=> Punished(punished, "muted", "unmuted", user, args);
		public static AdvobotResult VoiceMuted(bool punished, IUser user, PunishmentArgs args)
			=> Punished(punished, "voice muted", "unvoice muted", user, args);
		public static AdvobotResult Deafened(bool punished, IUser user, PunishmentArgs args)
			=> Punished(punished, "deafened", "undeafened", user, args);
		public static AdvobotResult Kicked(IUser user)
			=> Punished(true, "kicked", "", user, null);
		public static AdvobotResult SoftBanned(IUser user)
			=> Punished(true, "softbanned", "", user, null);
		public static AdvobotResult Banned(IUser user)
			=> Punished(true, "banned", "", user, null);
		public static AdvobotResult Unbanned(IBan ban)
			=> Punished(false, "", "unbanned", ban.User, null);
		public static AdvobotResult AlreadyInChannel(IUser user, IVoiceChannel channel)
			=> Failure(Default.FormatInterpolated($"{user} is already in {channel}.")).WithTime(DefaultTime);
		public static AdvobotResult Moved(IUser user, IVoiceChannel channel)
			=> Success(Default.FormatInterpolated($"Successfully moved {user} to {channel}."));
		public static AdvobotResult Pruned(int days, int amount)
			=> Success(Default.FormatInterpolated($"Successfully pruned {amount} members with a prune period of {days} days."));
		public static AdvobotResult FakePruned(int days, int amount)
			=> Success(Default.FormatInterpolated($"Successfully would have pruned {amount} members with a prune period of {days} days."));
		public static AdvobotResult DisplayBanReason(IBan ban)
		{
			return Success(new EmbedWrapper
			{
				Title = Title.FormatInterpolated($"Ban Reason for {ban.User}"),
				Description = ban.Reason ?? "No reason listed.",
			});
		}
		public static AdvobotResult DisplayBans(IReadOnlyCollection<IBan> bans)
		{
			var padLen = bans.Count.ToString().Length;
			return Success(new EmbedWrapper
			{
				Title = "Bans",
				Description = BigBlock.FormatInterpolated($"{bans.Select((x, i) => $"{i.ToString().PadLeft(padLen, '0')}. {x.User}")}"),
			});
		}
		public static AdvobotResult RemovedMessages(ITextChannel channel, IUser? user, int deleted)
		{
			if (user == null)
			{
				return Success(Default.FormatInterpolated($"Successfully deleted {deleted} message(s) on {channel}."));
			}
			return Success(Default.FormatInterpolated($"Successfully deleted {deleted} message(s) on {channel} from {user}."));
		}
		public static AdvobotResult CannotGiveGatheredRole()
			=> Failure("Cannot give the role being gathered.").WithTime(DefaultTime);
		public static AdvobotResult MultiUserAction(int amountLeft)
			=> Success(Default.FormatInterpolated($"Attempting to modify {amountLeft} users. ETA on completion: {(int)(amountLeft * 1.2)} seconds."));
		public static AdvobotResult MultiUserActionSuccess(int modified)
			=> Success(Default.FormatInterpolated($"Successfully modified {modified} users."));

		private static AdvobotResult Punished(bool punished, string punishment, string unpunishment, IUser user, PunishmentArgs? args)
		{
			if (punished)
			{
				var str = Default.FormatInterpolated($"Successfully {punishment.NoFormatting()} {user}.");
				if (args != null && args.Time != null && args.Timers != null)
				{
					str += Default.FormatInterpolated($" They will be {unpunishment.NoFormatting()} in {args.Time:0:00:00:00}.");
				}
				return Success(str);
			}
			return Success(Default.FormatInterpolated($"Successfully {unpunishment.NoFormatting()} {user}."));
		}
	}
}
