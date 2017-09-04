using Advobot.Actions;
using Advobot.Enums;
using Advobot.Interfaces;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Advobot.Classes
{
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

	public class CommandSwitch : ISetting
	{
		[JsonProperty]
		public string Name { get; }
		[JsonIgnore]
		public string[] Aliases { get; }
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
			Aliases = helpEntry.Aliases;
		}

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

		public void Errored(string errorReason)
		{
			ErrorReason = errorReason;
			WriteColor = ConsoleColor.Red;
		}
		public void Finished()
		{
			TimeCompleted = DateTime.UtcNow;
			Write();
		}
		public void Write()
		{
			ConsoleActions.WriteLine(this.ToString(), nameof(LoggedCommand), WriteColor);
		}

		public override string ToString()
		{
			var response = new System.Text.StringBuilder();
			response.Append($"Guild: {Guild}");
			response.Append($"{_Joiner}Channel: {Channel}");
			response.Append($"{_Joiner}User: {User}");
			response.Append($"{_Joiner}Time: {Time}");
			response.Append($"{_Joiner}Text: {Text}");
			response.Append($"{_Joiner}Time taken: {(TimeCompleted - TimeInitiated).TotalMilliseconds}ms");
			if (ErrorReason != null)
			{
				response.Append($"{_Joiner}Error: {ErrorReason}");
			}
			return response.ToString();
		}
	}
}
