
using Advobot.Invites.Models;
using Advobot.Utilities;

namespace Advobot.Invites.Utilities
{
	public static class InviteUtils
	{
		public static DateTimeOffset GetLastBumped(this ListedInvite invite)
			=> invite.LastBumped.CreateUtcDTOFromTicks();

		public static string GetUrl(this ListedInvite invite)
			=> "https://www.discord.gg/" + invite.Code;
	}
}