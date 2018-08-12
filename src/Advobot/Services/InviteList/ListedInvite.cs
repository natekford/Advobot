﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Interfaces;
using AdvorangesUtils;
using Discord;
using Discord.WebSocket;

namespace Advobot.Services.InviteList
{
	/// <summary>
	/// Lists an invite for use in <see cref="IInviteListService"/>.
	/// </summary>
	internal class ListedInvite : DatabaseEntry, IListedInvite
	{
		/// <inheritdoc />
		public string Code { get; set; }
		/// <inheritdoc />
		public bool Expired { get; set; }
		/// <inheritdoc />
		public ulong GuildId { get; set; }
		/// <inheritdoc />
		public int GuildMemberCount { get; set; }
		/// <inheritdoc />
		public string GuildName { get; set; }
		/// <inheritdoc />
		public bool HasGlobalEmotes { get; set; }
		/// <inheritdoc />
		public string[] Keywords { get; set; }
		/// <inheritdoc />
		public string Url => "https://www.discord.gg/" + Code;

		/// <summary>
		/// Creates an instance of listed invites.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="invite"></param>
		/// <param name="keywords"></param>
		public ListedInvite(SocketGuild guild, IInvite invite, IEnumerable<string> keywords) : base(default)
		{
			Code = invite.Code;
			Keywords = (keywords ?? Enumerable.Empty<string>()).ToArray();
			Update(guild);
		}

		/// <inheritdoc />
		public async Task BumpAsync(SocketGuild guild)
		{
			Time = DateTime.UtcNow;
			await UpdateAsync(guild).CAF();
		}
		/// <inheritdoc />
		public async Task UpdateAsync(SocketGuild guild)
		{
			Expired = !(await guild.GetInvitesAsync().CAF()).Any(x => x.Code == Code);
			Update(guild);
		}
		private void Update(SocketGuild guild)
		{
			GuildId = guild.Id;
			GuildMemberCount = guild.MemberCount;
			GuildName = guild.Name;
			HasGlobalEmotes = guild.Emotes.Any(x => x.IsManaged && x.RequireColons);
		}
		/// <inheritdoc />
		public override string ToString()
		{
			return $"**Code:** `{Code}`{(Keywords.Any() ? $"\n**Keywords:** `{String.Join("`, `", Keywords)}`" : "")}";
		}
	}
}
