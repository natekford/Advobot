using Advobot.Modules;
using Advobot.Utilities;

using Discord;

using static Advobot.Resources.Responses;
using static Advobot.Utilities.FormattingUtils;

namespace Advobot.Standard.Responses;

public sealed class Users : AdvobotResult
{
	public static AdvobotResult AlreadyInChannel(IUser user, IVoiceChannel channel)
	{
		return Success(UsersAlreadyInChannel.Format(
			user.Format().WithBlock(),
			channel.Format().WithBlock()
		));
	}

	public static AdvobotResult Banned(IUser user, TimeSpan? time)
		=> Punished(true, VariableBanned, VariableUnbanned, Format(user), time);

	public static AdvobotResult CannotFindUser(ulong userId)
		=> Failure(UsersDoesNotExist.Format(Format(userId)));

	public static AdvobotResult CannotGiveGatheredRole()
			=> Failure(UsersCannotGiveRoleBeingGathered);

	public static AdvobotResult Deafened(bool punished, IUser user, TimeSpan? time)
		=> Punished(punished, VariableDeafened, VariableUndeafened, Format(user), time);

	public static AdvobotResult Kicked(IUser user)
		=> Punished(true, VariableKicked, string.Empty, Format(user), null);

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

	public static AdvobotResult Muted(bool punished, IUser user, TimeSpan? time)
		=> Punished(punished, VariableMuted, VariableUnmuted, Format(user), time);

	public static AdvobotResult RemovedMessages(
		ITextChannel channel,
		IUser? user,
		int deleted)
	{
		if (user is null)
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

	public static AdvobotResult SoftBanned(ulong userId)
		=> Punished(true, VariableSoftBanned, string.Empty, Format(userId), null);

	public static AdvobotResult Unbanned(IUser user)
		=> Punished(false, VariableBanned, VariableUnbanned, Format(user), null);

	public static AdvobotResult VoiceMuted(bool punished, IUser user, TimeSpan? time)
		=> Punished(punished, VariableVoiceMuted, VariableUnvoiceMuted, Format(user), time);

	private static MarkdownString Format(IUser user)
		=> user.Format().WithBlock();

	private static MarkdownString Format(ulong userId)
		=> userId.ToString().WithBlock();

	private static AdvobotResult Punished(
		bool punished,
		string punishment,
		string unpunishment,
		MarkdownString users,
		TimeSpan? time)
	{
		var action = punished ? punishment : unpunishment;
		var str = UsersActionDone.Format(
			action.WithNoMarkdown(),
			users
		);

		if (time is TimeSpan ts)
		{
			str += " " + UsersUnpunishedTime.Format(
				unpunishment.WithNoMarkdown(),
				ts.ToString("0:00:00:00").WithBlock()
			);
		}
		return Success(str);
	}
}