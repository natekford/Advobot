using Advobot.Enums;
using Advobot.Interfaces;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Advobot.Classes
{
	public class Quote : ISetting, INameAndText
	{
		[JsonProperty]
		public string Name { get; }
		[JsonProperty]
		public string Text { get; }

		public Quote(string name, string text)
		{
			Name = name;
			Text = text;
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

	public class HelpEntry : INameAndText
	{
		public string Name { get; }
		public string[] Aliases { get; }
		public string Usage { get; }
		public string BasePerm { get; }
		public string Text { get; }
		public CommandCategory Category { get; }
		public bool DefaultEnabled { get; }
		private const string PLACE_HOLDER_STR = "N/A";

		public HelpEntry(string name, string[] aliases, string usage, string basePerm, string text, CommandCategory category, bool defaultEnabled)
		{
			Name = String.IsNullOrWhiteSpace(name) ? PLACE_HOLDER_STR : name;
			Aliases = aliases ?? new[] { PLACE_HOLDER_STR };
			Usage = String.IsNullOrWhiteSpace(usage) ? PLACE_HOLDER_STR : Constants.BOT_PREFIX + usage;
			BasePerm = String.IsNullOrWhiteSpace(basePerm) ? PLACE_HOLDER_STR : basePerm;
			Text = String.IsNullOrWhiteSpace(text) ? PLACE_HOLDER_STR : text;
			Category = category;
			DefaultEnabled = defaultEnabled;
		}

		public override string ToString()
		{
			var aliasStr = $"**Aliases:** {String.Join(", ", Aliases)}";
			var usageStr = $"**Usage:** {Usage}";
			var permStr = $"\n**Base Permission(s):**\n{BasePerm}";
			var descStr = $"\n**Description:**\n{Text}";
			return String.Join("\n", new[] { aliasStr, usageStr, permStr, descStr });
		}
	}

	/// <summary>
	/// Container of close words which is intended to be removed after <see cref="GetTime()"/> returns a time less than the current time.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public struct ActiveCloseWord<T> : ITimeInterface where T : INameAndText
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
	public struct CloseWord<T> where T : INameAndText
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
