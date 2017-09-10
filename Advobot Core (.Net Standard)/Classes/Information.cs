using Advobot.Enums;
using Advobot.Interfaces;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Advobot.Classes
{
	/// <summary>
	/// Holds a name and description.
	/// </summary>
	public class Quote : ISetting, IDescription
	{
		[JsonProperty]
		public string Name { get; }
		[JsonProperty]
		public string Description { get; }

		public Quote(string name, string description)
		{
			Name = name;
			Description = description;
		}

		public override string ToString()
		{
			return $"`{Name}`";
		}
		public string ToString(SocketGuild guild)
		{
			return ToString();
		}
	}

	/// <summary>
	/// Holds information about a command, such as its name, aliases, usage, base permissions, description, category, and default enabled value.
	/// </summary>
	public class HelpEntry : IDescription
	{
		private const string PLACE_HOLDER_STR = "N/A";
		public string Name { get; }
		public string[] Aliases { get; }
		public string Usage { get; }
		public string BasePerm { get; }
		public string Description { get; }
		public CommandCategory Category { get; }
		public bool DefaultEnabled { get; }

		public HelpEntry(string name, string[] aliases, string usage, string basePerm, string description, CommandCategory category, bool defaultEnabled)
		{
			Name = String.IsNullOrWhiteSpace(name) ? PLACE_HOLDER_STR : name;
			Aliases = aliases ?? new[] { PLACE_HOLDER_STR };
			Usage = String.IsNullOrWhiteSpace(usage) ? PLACE_HOLDER_STR : Constants.BOT_PREFIX + usage;
			BasePerm = String.IsNullOrWhiteSpace(basePerm) ? PLACE_HOLDER_STR : basePerm;
			Description = String.IsNullOrWhiteSpace(description) ? PLACE_HOLDER_STR : description;
			Category = category;
			DefaultEnabled = defaultEnabled;
		}

		public override string ToString()
		{
			var aliasStr = $"**Aliases:** {String.Join(", ", Aliases)}";
			var usageStr = $"**Usage:** {Usage}";
			var permStr = $"\n**Base Permission(s):**\n{BasePerm}";
			var descStr = $"\n**Description:**\n{Description}";
			return String.Join("\n", new[] { aliasStr, usageStr, permStr, descStr });
		}
	}

	/// <summary>
	/// Container of close words which is intended to be removed after <see cref="GetTime()"/> returns a value less than the current time.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public struct ActiveCloseWord<T> : ITimeInterface where T : IDescription
	{
		public ulong UserId { get; }
		public List<CloseWord<T>> List { get; }
		private DateTime _Time;

		public ActiveCloseWord(ulong userID, IEnumerable<CloseWord<T>> list)
		{
			UserId = userID;
			List = list.ToList();
			_Time = DateTime.UtcNow.AddSeconds(Constants.SECONDS_ACTIVE_CLOSE);
		}

		public DateTime GetTime()
		{
			return _Time;
		}
	}

	/// <summary>
	/// Holds an object which has a name and text and its closeness.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public struct CloseWord<T> where T : IDescription
	{
		public T Word { get; }
		public int Closeness { get; }

		public CloseWord(T word, int closeness)
		{
			Word = word;
			Closeness = closeness;
		}
	}
}
