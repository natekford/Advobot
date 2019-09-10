using System;

using Advobot.Invites.ReadOnlyModels;
using Advobot.Invites.Relationships;
using Advobot.Utilities;

namespace Advobot.Invites.Utilities
{
	public static class InviteUtils
	{
		public static ulong GetGuildId(this IGuildChild child)
			=> ulong.Parse(child.GuildId);

		public static DateTimeOffset GetLastBumped(this IReadOnlyListedInvite invite)
			=> invite.LastBumped.CreateUtcDTOFromTicks();

		public static string GetUrl(this IReadOnlyListedInvite invite)
			=> "https://www.discord.gg/" + invite.Code;
	}
}