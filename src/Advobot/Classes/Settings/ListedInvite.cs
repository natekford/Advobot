using Advobot.Interfaces;
using Advobot.Utilities;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Advobot.Classes.Settings
{
	/// <summary>
	/// Lists an invite for use in <see cref="IInviteListService"/>.
	/// </summary>
	public class ListedInvite : IGuildSetting
	{
		/// <summary>
		/// The code of the invite.
		/// </summary>
		[JsonProperty]
		public string Code;
		/// <summary>
		/// The keywords to use for the invite.
		/// </summary>
		[JsonProperty]
		public string[] Keywords;
		/// <summary>
		/// The url of the invite.
		/// </summary>
		[JsonIgnore]
		public string Url => "https://www.discord.gg/" + Code;
		/// <summary>
		/// The guild this invite is for.
		/// </summary>
		[JsonIgnore]
		public SocketGuild Guild { get; private set; }
		/// <summary>
		/// When the invite was last bumped.
		/// </summary>
		[JsonIgnore]
		public DateTime LastBumped { get; private set; }
		/// <summary>
		/// If the guild has global emotes.
		/// </summary>
		[JsonIgnore]
		public bool HasGlobalEmotes { get; private set; }

		/// <summary>
		/// Creates an instance of listed invites.
		/// </summary>
		/// <param name="invite"></param>
		/// <param name="keywords"></param>
		[JsonConstructor]
		public ListedInvite(IInvite invite, IEnumerable<string> keywords)
		{
			LastBumped = DateTime.UtcNow;
			Code = invite.Code;
			Keywords = (keywords ?? Enumerable.Empty<string>()).ToArray();
		}
		/// <summary>
		/// Creates an instance of listed invite.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="invite"></param>
		/// <param name="keywords"></param>
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

		/// <inheritdoc />
		public override string ToString()
		{
			return String.IsNullOrWhiteSpace(Code)
				? null : $"**Code:** `{Code}`{(Keywords.Any() ? $"\n**Keywords:** `{String.Join("`, `", Keywords)}`" : "")}";
		}
		/// <inheritdoc />
		public string ToString(SocketGuild guild)
		{
			return ToString();
		}
	}
}
