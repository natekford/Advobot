using Advobot.Core.Actions;
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
			this.LastBumped = DateTime.UtcNow;
			this.Code = invite.Code;
			this.Url = "https://www.discord.gg/" + this.Code;
			this.Keywords = (keywords ?? Enumerable.Empty<string>()).ToList();
		}
		public ListedInvite(SocketGuild guild, IInvite invite, IEnumerable<string> keywords) : this(invite, keywords)
		{
			this.Guild = guild;
			this.HasGlobalEmotes = this.Guild.HasGlobalEmotes();
		}

		/// <summary>
		/// Sets <see cref="Code"/> to <paramref name="code"/> and updates <see cref="Url"/> to have the new code.
		/// </summary>
		/// <param name="code"></param>
		public void UpdateCode(string code)
		{
			this.Code = code;
			this.Url = "https://www.discord.gg/" + this.Code;
		}
		/// <summary>
		/// Sets <see cref="Keywords"/> to <paramref name="keywords"/>.
		/// </summary>
		/// <param name="keywords"></param>
		public void UpdateKeywords(IEnumerable<string> keywords) => this.Keywords = keywords.ToList().AsReadOnly();
		/// <summary>
		/// Sets <see cref="LastBumped"/> to <see cref="DateTime.UtcNow"/> and checks for global emotes.
		/// </summary>
		public void UpdateLastBumped()
		{
			this.LastBumped = DateTime.UtcNow;
			this.HasGlobalEmotes = this.Guild.HasGlobalEmotes();
		}
		/// <summary>
		/// Sets the <see cref="Guild"/> property and checks for global emotes.
		/// </summary>
		/// <param name="guild"></param>
		public void PostDeserialize(SocketGuild guild)
		{
			this.Guild = guild;
			this.HasGlobalEmotes = this.Guild.HasGlobalEmotes();
		}

		public override string ToString()
			=> String.IsNullOrWhiteSpace(this.Code)
			? null
			: $"**Code:** `{this.Code}`{(this.Keywords.Any() ? $"\n**Keywords:** `{String.Join("`, `", this.Keywords)}`" : "")}";
		public string ToString(SocketGuild guild) => ToString();
	}
}
