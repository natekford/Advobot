using Advobot.Invites.Models;
using Advobot.Invites.Utilities;
using Advobot.Modules;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;

using System.Text;

using static Advobot.Resources.Responses;

namespace Advobot.Invites.Responses;

public sealed class Invites : AdvobotResult
{
	private Invites() : base(null, "")
	{
	}

	public static AdvobotResult Bumped()
		=> Success(ListedInviteBumped);

	public static AdvobotResult CreatedListing(IInviteMetadata invite)
	{
		return Success(ListedInviteCreated.Format(
			invite.Format().WithBlock()
		));
	}

	public static AdvobotResult DeletedListing()
		=> Success(ListedInviteDeleted);

	public static AdvobotResult InviteMatches(IEnumerable<ListedInvite> invites)
	{
		const string GHEADER = "Guild Name";
		const string MHEADER = "Member Count";
		const string UHEADER = "Url";
		const string EHEADER = "Global Emotes";
		const string YES = "Yes";

		var gLen = GHEADER.Length;
		var mLen = MHEADER.Length;
		var uLen = UHEADER.Length;
		var eLen = 0;
		foreach (var invite in invites)
		{
			gLen = Math.Max(gLen, invite.Name.Length);
			mLen = Math.Max(mLen, invite.MemberCount.ToString().Length);
			uLen = Math.Max(uLen, invite.GetUrl().Length);
			if (invite.HasGlobalEmotes)
			{
				eLen = EHEADER.Length;
			}
		}

		var sb = new StringBuilder();
		sb.Append(GHEADER.PadRight(gLen), 0, gLen);
		sb.Append(MHEADER.PadRight(mLen), 0, mLen);
		sb.Append(UHEADER.PadRight(uLen), 0, uLen);
		sb.Append(EHEADER.PadRight(eLen), 0, eLen);

		foreach (var invite in invites)
		{
			sb.Append(invite.Name.PadRight(gLen), 0, gLen);
			sb.Append(invite.GetUrl().PadRight(uLen), 0, uLen);
			sb.Append(invite.MemberCount.ToString().PadRight(mLen), 0, mLen);
			if (invite.HasGlobalEmotes)
			{
				sb.Append(YES);
			}
			sb.AppendLineFeed();
		}
		return Success(sb.ToString().WithBigBlock().Value);
	}

	public static AdvobotResult NoInviteMatch()
		=> Failure(ListedInviteNoInviteMatch);

	public static AdvobotResult TooManyMatches()
		=> Failure(ListedInviteTooManyMatches);
}