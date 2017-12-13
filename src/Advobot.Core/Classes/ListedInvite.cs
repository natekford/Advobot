using Advobot.Core.Utilities;
using Advobot.Core.Interfaces;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Lists an invite for use in <see cref="IInviteListService"/>.
	/// </summary>
	public class ListedInvite : ISetting
	{
		[JsonProperty]
		public string Code { get; private set; }
		[JsonProperty]
		public IReadOnlyList<string> Keywords { get; private set; }
		[JsonProperty]
		public bool HasGlobalEmotes { get; private set; }
		[JsonIgnore]
		public DateTime LastBumped { get; private set; }
		[JsonIgnore]
		public string Url { get; private set; }
		[JsonIgnore]
		public SocketGuild Guild { get; private set; }

		[JsonConstructor]
		public ListedInvite(IInvite invite, IEnumerable<string> keywords)
		{
			LastBumped = DateTime.UtcNow;
			Code = invite.Code;
			Url = "https://www.discord.gg/" + Code;
			Keywords = (keywords ?? Enumerable.Empty<string>()).ToList();
		}
		public ListedInvite(SocketGuild guild, IInvite invite, IEnumerable<string> keywords) : this(invite, keywords)
		{
			Guild = guild;
			HasGlobalEmotes = Guild.HasGlobalEmotes();
		}

		/// <summary>
		/// Sets <see cref="Code"/> to <paramref name="code"/> and updates <see cref="Url"/> to have the new code.
		/// </summary>
		/// <param name="code"></param>
		public void UpdateCode(string code)
		{
			Code = code;
			Url = "https://www.discord.gg/" + Code;
		}
		/// <summary>
		/// Sets <see cref="Keywords"/> to <paramref name="keywords"/>.
		/// </summary>
		/// <param name="keywords"></param>
		public void UpdateKeywords(IEnumerable<string> keywords) => Keywords = keywords.ToList().AsReadOnly();
		/// <summary>
		/// Sets <see cref="LastBumped"/> to <see cref="DateTime.UtcNow"/> and checks for global emotes.
		/// </summary>
		public void UpdateLastBumped()
		{
			LastBumped = DateTime.UtcNow;
			HasGlobalEmotes = Guild.HasGlobalEmotes();
		}
		/// <summary>
		/// Sets the <see cref="Guild"/> property and checks for global emotes.
		/// </summary>
		/// <param name="guild"></param>
		public void PostDeserialize(SocketGuild guild)
		{
			Guild = guild;
			HasGlobalEmotes = Guild.HasGlobalEmotes();
		}

		public override string ToString()
			=> String.IsNullOrWhiteSpace(Code)
			? null
			: $"**Code:** `{Code}`{(Keywords.Any() ? $"\n**Keywords:** `{String.Join("`, `", Keywords)}`" : "")}";
		public string ToString(SocketGuild guild) => ToString();
	}
}
