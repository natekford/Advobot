using Advobot.Actions;
using Advobot.Interfaces;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Advobot.Classes
{
	public class ListedInvite : ISetting
	{
		[JsonProperty]
		public string Code { get; private set; }
		[JsonProperty]
		public string[] Keywords { get; private set; }
		[JsonProperty]
		public bool HasGlobalEmotes { get; private set; }
		[JsonIgnore]
		public DateTime LastBumped { get; private set; }
		[JsonIgnore]
		public string Url { get; private set; }
		[JsonIgnore]
		public SocketGuild Guild { get; private set; }

		[JsonConstructor]
		public ListedInvite(string code, string[] keywords)
		{
			LastBumped = DateTime.UtcNow;
			Code = code;
			Url = "https://www.discord.gg/" + Code;
			Keywords = keywords ?? new string[0];
		}
		public ListedInvite(SocketGuild guild, string code, string[] keywords) : this(code, keywords)
		{
			Guild = guild;
			HasGlobalEmotes = Guild.HasGlobalEmotes();
		}

		public void UpdateCode(string code)
		{
			Code = code;
			Url = "https://www.discord.gg/" + Code;
		}
		public void UpdateKeywords(string[] keywords)
		{
			Keywords = keywords;
		}
		public void UpdateLastBumped()
		{
			LastBumped = DateTime.UtcNow;
		}
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

			var codeStr = $"**Code:** `{Code}`\n";
			var keywordStr = "";
			if (Keywords.Any())
			{
				keywordStr = $"**Keywords:**\n`{String.Join("`, `", Keywords)}`";
			}
			return codeStr + keywordStr;
		}
		public string ToString(SocketGuild guild)
		{
			return ToString();
		}
	}


	public class BotInvite
	{
		public ulong GuildId { get; }
		public string Code { get; }
		public int Uses { get; private set; }

		public BotInvite(ulong guildId, string code, int uses)
		{
			GuildId = guildId;
			Code = code;
			Uses = uses;
		}

		public void IncrementUses()
		{
			++Uses;
		}
	}
}
