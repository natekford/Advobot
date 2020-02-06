using System;
using System.Collections.Generic;
using System.Linq;

using Advobot.Classes;
using Advobot.Modules;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;

using static Advobot.Standard.Resources.Responses;
using static Advobot.Utilities.FormattingUtils;

namespace Advobot.Standard.Responses
{
	public sealed class Users : AdvobotResult
	{
		private Users() : base(null, "")
		{
		}

		public static AdvobotResult AlreadyInChannel(IUser user, IVoiceChannel channel)
		{
			return Success(UsersAlreadyInChannel.Format(
				user.Format().WithBlock(),
				channel.Format().WithBlock()
			));
		}

		public static AdvobotResult Banned(bool punished, IUser user, PunishmentArgs args)
			=> Punished(punished, UsersVariableBanned, UsersVariableUnbanned, F(user), args);

		public static AdvobotResult BannedMany(IEnumerable<IUser> users, PunishmentArgs args)
			=> Punished(true, UsersVariableBanned, UsersVariableUnbanned, F(users), args);

		public static AdvobotResult CannotGiveGatheredRole()
			=> Failure(UsersCannotGiveRoleBeingGathered);

		public static AdvobotResult Deafened(bool punished, IUser user, PunishmentArgs args)
			=> Punished(punished, UsersVariableDeafened, UsersVariableUndeafened, F(user), args);

		public static AdvobotResult DisplayBanReason(IBan ban)
		{
			var title = UsersTitleBanReason.Format(
				ban.User.Format().WithNoMarkdown()
			);
			return Success(new EmbedWrapper
			{
				Title = title,
				Description = ban.Reason ?? UsersVariableNoBanReason,
			});
		}

		public static AdvobotResult DisplayBans(IReadOnlyCollection<IBan> bans)
		{
			var padLen = bans.Count.ToString().Length;
			var description = bans
				.Select((x, i) => (Position: i, Ban: x))
				.Join(x => $"{x.Position.ToString().PadRight(padLen)} {x.Ban.User.Format()}", Environment.NewLine)
				.WithBigBlock()
				.Value;
			return Success(new EmbedWrapper
			{
				Title = UsersTitleBans,
				Description = description,
			});
		}

		public static AdvobotResult FakePruned(int days, int amount)
		{
			return Success(UsersFakePrune.Format(
				amount.ToString().WithBlock(),
				days.ToString().WithBlock()
			));
		}

		public static AdvobotResult Kicked(IUser user)
			=> Punished(true, UsersVariableKicked, string.Empty, F(user), null);

		public static AdvobotResult Moved(IUser user, IVoiceChannel channel)
		{
			return Success(UsersMoved.Format(
				user.Format().WithBlock(),
				channel.Format().WithBlock()
			));
		}

		public static AdvobotResult MultiUserActionProgress(int amountLeft)
		{
			return Success(UsersMultiUserActionProgress.Format(
				amountLeft.ToString().WithBlock(),
				((int)(amountLeft * 1.2)).ToString().WithBlock()
			));
		}

		public static AdvobotResult MultiUserActionSuccess(int modified)
		{
			return Success(UsersMultiUserActionSuccess.Format(
				modified.ToString().WithBlock()
			));
		}

		public static AdvobotResult Muted(bool punished, IUser user, PunishmentArgs args)
			=> Punished(punished, UsersVariableMuted, UsersVariableUnmuted, F(user), args);

		public static AdvobotResult Pruned(int days, int amount)
		{
			return Success(UsersRealPrune.Format(
				amount.ToString().WithBlock(),
				days.ToString().WithBlock()
			));
		}

		public static AdvobotResult RemovedMessages(
			ITextChannel channel,
			IUser? user,
			int deleted)
		{
			if (user == null)
			{
				return Success(UsersDeletedMessagesNoTarget.Format(
					deleted.ToString().WithBlock(),
					channel.Format().WithBlock()
				));
			}
			return Success(UsersDeletedMessages.Format(
				deleted.ToString().WithBlock(),
				channel.Format().WithBlock(),
				user.Format().WithBlock()
			));
		}

		public static AdvobotResult SoftBanned(IUser user)
			=> Punished(true, UsersVariableSoftBanned, string.Empty, F(user), null);

		public static AdvobotResult VoiceMuted(bool punished, IUser user, PunishmentArgs args)
			=> Punished(punished, UsersVariableVoiceMuted, UsersVariableUnvoiceMuted, F(user), args);

		private static MarkdownFormattedArg F(IUser user)
			=> user.Format().WithBlock();

		private static MarkdownFormattedArg F(IEnumerable<IUser> users)
			=> users.Join(x => x.Format()).WithBlock();

		private static AdvobotResult Punished(
			bool punished,
			string punishment,
			string unpunishment,
			MarkdownFormattedArg users,
			PunishmentArgs? args)
		{
			var action = punished ? punishment : unpunishment;
			var str = UsersActionDone.Format(
				action.WithNoMarkdown(),
				users
			);

			if (punished && args?.Time is TimeSpan ts)
			{
				str += " " + UsersUnpunishedTime.Format(
					unpunishment.WithNoMarkdown(),
					ts.ToString("0:00:00:00").WithBlock()
				);
			}
			return Success(str);
		}
	}
}