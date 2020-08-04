using System;
using System.Linq;

using Advobot.Databases.Relationships;
using Advobot.Invites.ReadOnlyModels;
using Advobot.Utilities;

using Discord;

namespace Advobot.Invites.Models
{
	public sealed class ListedInvite : IReadOnlyListedInvite
	{
		public string Code { get; set; }
		public string GuildId { get; set; }
		public bool HasGlobalEmotes { get; set; }
		public long LastBumped { get; set; }
		public int MemberCount { get; set; }
		public string Name { get; set; }

		ulong IGuildChild.GuildId => GuildId.ToId();

		public ListedInvite()
		{
			Code = "";
			GuildId = "";
			Name = "";
		}

		public ListedInvite(
			IInviteMetadata invite,
			DateTimeOffset now) : this()
		{
			LastBumped = now.Ticks;
			Code = invite.Code;
			GuildId = invite.GuildId.ToString();
			MemberCount = invite.MemberCount ?? 1;
			Name = invite.GuildName;
			HasGlobalEmotes = invite.Guild.Emotes.Any(x => x.IsManaged && x.RequireColons);
		}
	}
}