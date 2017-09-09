using Advobot.Actions;
using Advobot.Interfaces;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Advobot.Classes
{
	/// <summary>
	/// Lists an invite for use in <see cref="IInviteListModule"/>.
	/// </summary>
	public class ListedInvite : ISetting
	{
		[JsonProperty]
		public string Code { get; private set; }
		[JsonProperty]
		public ReadOnlyCollection<string> Keywords { get; private set; }
		[JsonProperty]
		public bool HasGlobalEmotes { get; private set; }
		[JsonIgnore]
		public DateTime LastBumped { get; private set; }
		[JsonIgnore]
		public string Url { get; private set; }
		[JsonIgnore]
		public SocketGuild Guild { get; private set; }

		[JsonConstructor]
		public ListedInvite(string code, IEnumerable<string> keywords)
		{
			LastBumped = DateTime.UtcNow;
			Code = code;
			Url = "https://www.discord.gg/" + Code;
			Keywords = (keywords ?? Enumerable.Empty<string>()).ToList().AsReadOnly();
		}
		public ListedInvite(SocketGuild guild, string code, IEnumerable<string> keywords) : this(code, keywords)
		{
			Guild = guild;
			HasGlobalEmotes = Guild.HasGlobalEmotes();
			Keywords = (keywords ?? Enumerable.Empty<string>()).ToList().AsReadOnly();
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
		public void UpdateKeywords(IEnumerable<string> keywords)
		{
			Keywords = keywords.ToList().AsReadOnly();
		}
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
		{
			if (String.IsNullOrWhiteSpace(Code))
			{
				return null;
			}
			return $"**Code:** `{Code}`\n" + (Keywords.Any() ? $"**Keywords:**\n`{String.Join("`, `", Keywords)}`" : "");
		}
		public string ToString(SocketGuild guild)
		{
			return ToString();
		}
	}

	/// <summary>
	/// Holds the code and uses of an invite. Used in <see cref="InviteActions.GetInviteUserJoinedOn(IGuildSettings, Discord.IGuild)"/>.
	/// </summary>
	public class BotInvite
	{
		public string Code { get; }
		public int Uses { get; private set; }

		public BotInvite(string code, int uses)
		{
			Code = code;
			Uses = uses;
		}

		public void IncrementUses()
		{
			++Uses;
		}
	}
}
