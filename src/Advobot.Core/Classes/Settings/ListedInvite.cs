using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Advobot.Core.Classes.Settings
{
	/// <summary>
	/// Lists an invite for use in <see cref="IInviteListService"/>.
	/// </summary>
	public class ListedInvite : IGuildSetting
	{
		[JsonProperty]
		public string Code;
		[JsonProperty]
		public string[] Keywords;
		[JsonIgnore]
		public string Url => "https://www.discord.gg/" + Code;
		[JsonIgnore]
		public SocketGuild Guild { get; private set; }
		[JsonIgnore]
		public DateTime LastBumped { get; private set; }
		[JsonIgnore]
		public bool HasGlobalEmotes { get; private set; }

		[JsonConstructor]
		public ListedInvite(IInvite invite, IEnumerable<string> keywords)
		{
			LastBumped = DateTime.UtcNow;
			Code = invite.Code;
			Keywords = (keywords ?? Enumerable.Empty<string>()).ToArray();
		}
		public ListedInvite(SocketGuild guild, IInvite invite, IEnumerable<string> keywords) : this(invite, keywords)
		{
			Guild = guild;
		}

		/// <summary>
		/// Sets <see cref="LastBumped"/> to <see cref="DateTime.UtcNow"/> and checks for global emotes.
		/// </summary>
		public void UpdateLastBumped()
		{
			LastBumped = DateTime.UtcNow;
			HasGlobalEmotes = Guild.Emotes.Any(x => x.IsManaged && x.RequireColons);
		}
		/// <summary>
		/// Sets the <see cref="Guild"/> property and checks for global emotes.
		/// </summary>
		/// <param name="guild"></param>
		public void PostDeserialize(SocketGuild guild)
		{
			Guild = guild;
			UpdateLastBumped();
		}

		public override string ToString()
		{
			return String.IsNullOrWhiteSpace(Code)
				? null
				: $"**Code:** `{Code}`{(Keywords.Any() ? $"\n**Keywords:** `{String.Join("`, `", Keywords)}`" : "")}";
		}
		public string ToString(SocketGuild guild)
		{
			return ToString();
		}
	}
}
