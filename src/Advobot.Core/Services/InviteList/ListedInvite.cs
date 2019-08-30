using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Advobot.Databases.Abstract;

using AdvorangesUtils;

using Discord;
using Discord.WebSocket;

namespace Advobot.Services.InviteList
{
	/// <summary>
	/// Lists an invite for use in <see cref="IInviteListService"/>.
	/// </summary>
	internal sealed class ListedInvite : TimedDatabaseEntry<string>, IListedInvite
	{
		/// <summary>
		/// Creates an instance of listed invites.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="invite"></param>
		/// <param name="keywords"></param>
		public ListedInvite(
			SocketGuild guild,
			IInviteMetadata invite,
			IEnumerable<string> keywords)
			: base(invite.Code, TimeSpan.FromHours(1))
		{
			Code = invite.Code;
			Keywords = (keywords ?? Enumerable.Empty<string>()).ToArray();
			GuildId = guild.Id;
			GuildMemberCount = guild.MemberCount;
			GuildName = guild.Name;
			HasGlobalEmotes = guild.Emotes.Any(x => x.IsManaged && x.RequireColons);
		}

		/// <inheritdoc />
		public string Code { get; private set; }

		/// <inheritdoc />
		public bool Expired { get; private set; }

		/// <inheritdoc />
		public ulong GuildId { get; private set; }

		/// <inheritdoc />
		public int GuildMemberCount { get; private set; }

		/// <inheritdoc />
		public string GuildName { get; private set; }

		/// <inheritdoc />
		public bool HasGlobalEmotes { get; private set; }

		/// <inheritdoc />
		public string[] Keywords { get; set; }

		/// <inheritdoc />
		public string Url => "https://www.discord.gg/" + Code;

		/// <inheritdoc />
		public Task BumpAsync(SocketGuild guild)
		{
			Time = DateTime.UtcNow;
			return UpdateAsync(guild);
		}

		/// <inheritdoc />
		public override string ToString()
			=> $"**Code:** `{Code}`{(Keywords.Length > 0 ? $"\n**Keywords:** `{string.Join("`, `", Keywords)}`" : "")}";

		/// <inheritdoc />
		public async Task UpdateAsync(SocketGuild guild)
		{
			Expired = !(await guild.GetInvitesAsync().CAF()).Any(x => x.Code == Code);
			GuildId = guild.Id;
			GuildMemberCount = guild.MemberCount;
			GuildName = guild.Name;
			HasGlobalEmotes = guild.Emotes.Any(x => x.IsManaged && x.RequireColons);
		}
	}
}