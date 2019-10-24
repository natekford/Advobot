using System;

using Advobot.Invites.ReadOnlyModels;
using Advobot.Utilities;

namespace Advobot.Invites.Utilities
{
	public static class InviteUtils
	{
		public static DateTimeOffset GetLastBumped(this IReadOnlyListedInvite invite)
			=> invite.LastBumped.CreateUtcDTOFromTicks();

		public static string GetUrl(this IReadOnlyListedInvite invite)
			=> "https://www.discord.gg/" + invite.Code;
	}
}