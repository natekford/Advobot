using Advobot.Actions;
using Advobot.Enums;
using Advobot.Interfaces;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.ObjectModel;

namespace Advobot.Classes
{
	/// <summary>
	/// A setting on a guild that states that the command is off for whatever Discord entity has that Id.
	/// </summary>
	public class CommandOverride : ISetting
	{
		[JsonProperty]
		public string Name { get; }
		[JsonProperty]
		public ulong Id { get; }

		public CommandOverride(string name, ulong id)
		{
			Name = name;
			Id = id;
		}

		public override string ToString()
		{
			return $"**Command:** `{Name}`\n**ID:** `{Id}`";
		}
		public string ToString(SocketGuild guild)
		{
			return ToString();
		}
	}

	/// <summary>
	/// A setting on guilds that states whether a command is on or off.
	/// </summary>
	public class CommandSwitch : ISetting
	{
		[JsonProperty]
		public string Name { get; }
		[JsonIgnore]
		public ReadOnlyCollection<string> Aliases { get; }
		[JsonProperty]
		public bool Value { get; private set; }
		[JsonProperty]
		public CommandCategory Category { get; }

		[JsonIgnore]
		public string ValueAsString { get => Value ? "ON" : "OFF"; }
		[JsonIgnore]
		public string CategoryName { get => Category.EnumName(); }
		[JsonIgnore]
		public int CategoryValue { get => (int)Category; }

		public CommandSwitch(string name, bool value)
		{
			var helpEntry = Constants.HELP_ENTRIES.FirstOrDefault(x => x.Name.Equals(name));
			if (helpEntry == null)
			{
				//TODO: uncomment this when all commands have been put back in
				//throw new ArgumentException("Command name does not have a help entry.");
				return;
			}

			Name = name;
			Value = value;
			Category = helpEntry.Category;
			Aliases = helpEntry.Aliases.ToList().AsReadOnly();
		}

		/// <summary>
		/// Sets <see cref="Value"/> to its opposite.
		/// </summary>
		public void ToggleEnabled()
		{
			Value = !Value;
		}

		public override string ToString()
		{
			return $"`{ValueAsString.PadRight(3)}` `{Name}`";
		}
		public string ToString(SocketGuild guild)
		{
			return ToString();
		}
	}

	/// <summary>
	/// Holds information about a command. 
	/// </summary>
	public struct LoggedCommand
	{
		private static readonly string _Joiner = Environment.NewLine + new string(' ', 28);
		public string Guild { get; }
		public string Channel { get; }
		public string User { get; }
		public string Time { get; }
		public string Text { get; }
		public DateTime TimeInitiated { get; }
		public DateTime TimeCompleted { get; private set; }
		public string ErrorReason { get; private set; }
		public ConsoleColor WriteColor;

		public LoggedCommand(ICommandContext context, DateTime startTime)
		{
			Guild = context.Guild.FormatGuild();
			Channel = context.Channel.FormatChannel();
			User = context.User.FormatUser();
			Time = FormattingActions.FormatDateTime(context.Message.CreatedAt);
			Text = context.Message.Content;
			TimeInitiated = startTime;
			TimeCompleted = DateTime.UtcNow;
			ErrorReason = null;
			WriteColor = ConsoleColor.Green;
		}

		/// <summary>
		/// Sets <see cref="ErrorReason"/> to <paramref name="errorReason"/> and changes <see cref="WriteColor"/> to <see cref="ConsoleColor.Red"/>.
		/// </summary>
		/// <param name="errorReason"></param>
		public void Errored(string errorReason)
		{
			ErrorReason = errorReason;
			WriteColor = ConsoleColor.Red;
		}
		/// <summary>
		/// Sets <see cref="TimeCompleted"/> to <see cref="DateTime.UtcNow"/> and write the logged command to the console.
		/// </summary>
		public void Finished()
		{
			TimeCompleted = DateTime.UtcNow;
			Write();
		}
		/// <summary>
		/// Writes this to the console in whatever color <see cref="WriteColor"/> is.
		/// </summary>
		public void Write()
		{
			ConsoleActions.WriteLine(this.ToString(), nameof(LoggedCommand), WriteColor);
		}

		public override string ToString()
		{
			var response = new System.Text.StringBuilder()
				.Append($"Guild: {Guild}")
				.Append($"{_Joiner}Channel: {Channel}")
				.Append($"{_Joiner}User: {User}")
				.Append($"{_Joiner}Time: {Time}")
				.Append($"{_Joiner}Text: {Text}")
				.Append($"{_Joiner}Time taken: {(TimeCompleted - TimeInitiated).TotalMilliseconds}ms");
			if (ErrorReason != null)
			{
				response.Append($"{_Joiner}Error: {ErrorReason}");
			}
			return response.ToString();
		}
	}
}
