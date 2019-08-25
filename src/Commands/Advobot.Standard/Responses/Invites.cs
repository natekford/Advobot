using System;
using System.Collections.Generic;
using Advobot.Classes;
using Advobot.Modules;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using static Advobot.Standard.Resources.Responses;

namespace Advobot.Standard.Responses
{
	public sealed class Invites : CommandResponses
	{
		private Invites() { }

		public static AdvobotResult DisplayInvites(IReadOnlyList<IInviteMetadata> invites)
		{
			var codeLen = -1;
			var useLen = -1;
			foreach (var invite in invites)
			{
				codeLen = Math.Max(codeLen, invite.Code.Length);
				useLen = Math.Max(useLen, invite.Uses.ToString().Length);
			}

			string FormatInvite(IInviteMetadata i)
				=> $"{i.Code.PadRight(codeLen)} {i.Uses.ToString().PadRight(useLen)} {i.Inviter.Format()}";

			var description = invites
				.Join(FormatInvite, Environment.NewLine)
				.WithBigBlock()
				.Value;
			return Success(new EmbedWrapper
			{
				Title = InvitesTitleDisplay,
				Description = description,
			});
		}
		public static AdvobotResult NoInviteMatches()
			=> Failure(InvitesNoInviteMatches);
		public static AdvobotResult DeletedMultipleInvites(IReadOnlyCollection<IInviteMetadata> invites)
		{
			return Success(InvitesDeletedMultipleInvites.Format(
				invites.Count.ToString().WithBlock()
			));
		}
	}
}