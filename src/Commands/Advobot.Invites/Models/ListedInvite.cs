using System;
using System.Linq;

using Advobot.Invites.ReadOnlyModels;

using Discord;

namespace Advobot.Invites.Models
{
	public sealed class ListedInvite : IReadOnlyListedInvite
	{
		public string Code { get; set; }
		public ulong GuildId { get; set; }
		public bool HasGlobalEmotes { get; set; }
		public long LastBumped { get; set; }
		public int MemberCount { get; set; }
		public string Name { get; set; }

		public ListedInvite()
		{
			Name = "";
			Code = "";
		}

		public ListedInvite(
			IInviteMetadata invite,
			DateTimeOffset now)
		{
			LastBumped = now.Ticks;
			Code = invite.Code;
			GuildId = invite.GuildId ?? throw new ArgumentException(nameof(invite));
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