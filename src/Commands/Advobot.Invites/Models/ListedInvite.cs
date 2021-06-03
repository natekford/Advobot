using System;
using System.Linq;

using Advobot.SQLite.Relationships;

using Discord;

namespace Advobot.Invites.Models
{
	public sealed record ListedInvite(
		string Code,
		ulong GuildId,
		bool HasGlobalEmotes,
		long LastBumped,
		int MemberCount,
		string Name
	) : IGuildChild
	{
		public ListedInvite() : this("", default, default, default, default, "")
		{
		}

		public ListedInvite(
			IInviteMetadata invite,
			DateTimeOffset now) : this()
		{
			LastBumped = now.Ticks;
			Code = invite.Code;
			GuildId = invite.GuildId ?? throw new ArgumentException($"Invalid guild id for invite {invite.Code}.", nameof(invite));
			MemberCount = invite.MemberCount ?? 1;
			Name = invite.GuildName;

			try
			{
				HasGlobalEmotes = invite.Guild.Emotes.Any(x => x.IsManaged && x.RequireColons);
			}
			catch
			{
				HasGlobalEmotes = false;
			}
		}
	}
}