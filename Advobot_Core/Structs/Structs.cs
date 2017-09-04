using Advobot.Actions;
using Advobot.Enums;
using Advobot.Interfaces;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Advobot.Structs
{
	/// <summary>
	/// Holds a guild permission name and value.
	/// </summary>
	public struct BotGuildPermission : IPermission
	{
		public string Name { get; }
		public ulong Value { get; }

		public BotGuildPermission(string name, int position)
		{
			Name = name;
			Value = (1U << position);
		}
	}

	/// <summary>
	/// Holds a channel permission name and value. Also holds booleans describing whether or not the permissions is on text/voice/both channels.
	/// </summary>
	public struct BotChannelPermission : IPermission
	{
		public string Name { get; }
		public ulong Value { get; }
		public bool General { get; }
		public bool Text { get; }
		public bool Voice { get; }

		public BotChannelPermission(string name, int position, bool gen = false, bool text = false, bool voice = false)
		{
			Name = name;
			Value = (1U << position);
			General = gen;
			Text = text;
			Voice = voice;
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

	/// <summary>
	/// Basically a tuple of a FailureReason and a different object.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public struct ReturnedObject<T>
	{
		public T Object { get; }
		public FailureReason Reason { get; }

		public ReturnedObject(T obj, FailureReason reason)
		{
			Object = obj;
			Reason = reason;
		}
	}

	/// <summary>
	/// Holds a <see cref="DateTime"/> object and implements <see cref="ITimeInterface"/> so certain methods can restrict generics easier.
	/// </summary>
	public struct BasicTimeInterface : ITimeInterface
	{
		private DateTime _Time;

		public BasicTimeInterface(DateTime time)
		{
			_Time = time.ToUniversalTime();
		}

		public DateTime GetTime()
		{
			return _Time;
		}
	}

	/// <summary>
	/// Holds information about a command. 
	/// </summary>
	public struct LoggedCommand
	{
		private static readonly string _Joiner = Environment.NewLine + new string (' ', 28);
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

	/// <summary>
	/// Basically a tuple for three bools which represent critical information.
	/// </summary>
	public struct CriticalInformation
	{
		/// <summary>
		/// True if the system is windows, false otherwise.
		/// </summary>
		public bool Windows { get; }
		/// <summary>
		/// True if the program is in console mode, false otherwise.
		/// </summary>
		public bool Console { get; }
		/// <summary>
		/// True if the bot Id held in <see cref="Properties.Settings.Path"/> does not match the current bot's Id.
		/// </summary>
		public bool FirstInstance { get; }

		public CriticalInformation(bool windows, bool console, bool firstInstance)
		{
			Windows = windows;
			Console = console;
			FirstInstance = firstInstance;
		}
	}
}
