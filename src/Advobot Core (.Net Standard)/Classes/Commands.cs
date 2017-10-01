using Advobot.Actions;
using Advobot.Enums;
using Advobot.Actions.Formatting;
using Advobot.Interfaces;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

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
		[JsonProperty]
		public bool Value { get; private set; }
		[JsonIgnore]
		public ReadOnlyCollection<string> Aliases { get; }
		[JsonIgnore]
		public CommandCategory Category { get; }
		[JsonIgnore]
		public string ValueAsString { get => Value ? "ON" : "OFF"; }

		public CommandSwitch(string name, bool value)
		{
			var helpEntry = Constants.HELP_ENTRIES.FirstOrDefault(x => x.Name.Equals(name));
			if (helpEntry == null)
			{
				Category = default;
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
	public class LoggedCommand
	{
		private static readonly string _Joiner = Environment.NewLine + new string(' ', 28);

		public string Guild { get; private set; }
		public string Channel { get; private set; }
		public string User { get; private set; }
		public string Time { get; private set; }
		public string Text { get; private set; }
		public string ErrorReason { get; private set; }
		public ConsoleColor WriteColor { get; private set; } = ConsoleColor.Green;
		private Stopwatch Stopwatch;

		public LoggedCommand()
		{
			Stopwatch = new Stopwatch();
			Stopwatch.Start();
		}

		/// <summary>
		/// Updates the logged command with who did what and other information.
		/// </summary>
		/// <param name="context"></param>
		public void SetContext(ICommandContext context)
		{
			Guild = context.Guild.FormatGuild();
			Channel = context.Channel.FormatChannel();
			User = context.User.FormatUser();
			Time = TimeFormatting.FormatReadableDateTime(context.Message.CreatedAt.UtcDateTime);
			Text = context.Message.Content;
		}
		/// <summary>
		/// Attempts to get an error reason if the <paramref name="result"/> has <see cref="IResult.IsSuccess"/> false.
		/// </summary>
		/// <param name="result"></param>
		public void SetError(IResult result)
		{
			if (TryGetErrorReason(result, out string errorReason))
			{
				ErrorReason = errorReason;
				WriteColor = ConsoleColor.Red;
			}
		}
		/// <summary>
		/// Sets <see cref="TimeCompleted"/> to <see cref="DateTime.UtcNow"/>.
		/// </summary>
		public void Finalize(ICommandContext context, IResult result)
		{
			SetContext(context);
			SetError(result);
			Stopwatch.Stop();
		}
		/// <summary>
		/// Writes this to the console in whatever color <see cref="WriteColor"/> is.
		/// </summary>
		public void Write()
		{
			ConsoleActions.WriteLine(this.ToString(), nameof(LoggedCommand), WriteColor);
		}
		/// <summary>
		/// Returns true if there is a valid error reason. Returns false if the command executed without errors.
		/// </summary>
		/// <param name="result"></param>
		/// <param name="errorReason"></param>
		/// <returns></returns>
		private bool TryGetErrorReason(IResult result, out string errorReason)
		{
			errorReason = result.ErrorReason;
			if (result.IsSuccess || Constants.IGNORE_ERROR.CaseInsEquals(result.ErrorReason))
			{
				return false;
			}

			switch (result.Error)
			{
				case null:
				//Ignore commands with the unknown command error because it's annoying
				case CommandError.UnknownCommand:
				{
					return false;
				}
				default:
				{
					return true;
				}
			}
		}

		public override string ToString()
		{
			var response = new System.Text.StringBuilder()
				.Append($"Guild: {Guild}")
				.Append($"{_Joiner}Channel: {Channel}")
				.Append($"{_Joiner}User: {User}")
				.Append($"{_Joiner}Time: {Time}")
				.Append($"{_Joiner}Text: {Text}")
				.Append($"{_Joiner}Time taken: {Stopwatch.ElapsedMilliseconds}ms");
			if (ErrorReason != null)
			{
				response.Append($"{_Joiner}Error: {ErrorReason}");
			}
			return response.ToString();
		}
	}
}
