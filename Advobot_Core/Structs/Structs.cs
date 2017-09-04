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

	public struct ReturnedArguments
	{
		public List<string> Arguments { get; }
		public int ArgCount { get; }
		public Dictionary<string, string> SpecifiedArguments { get; }
		public List<ulong> MentionedUsers { get; }
		public List<ulong> MentionedRoles { get; }
		public List<ulong> MentionedChannels { get; }
		public FailureReason Reason { get; }

		public ReturnedArguments(List<string> args, FailureReason reason)
		{
			Arguments = args;
			ArgCount = args.Where(x => !String.IsNullOrWhiteSpace(x)).Count();
			SpecifiedArguments = null;
			MentionedUsers = null;
			MentionedRoles = null;
			MentionedChannels = null;
			Reason = reason;
		}
		public ReturnedArguments(List<string> args, Dictionary<string, string> specifiedArgs, IMessage message)
		{
			Arguments = args;
			ArgCount = args.Where(x => !String.IsNullOrWhiteSpace(x)).Count();
			SpecifiedArguments = specifiedArgs;
			MentionedUsers = message.MentionedUserIds.ToList();
			MentionedRoles = message.MentionedRoleIds.ToList();
			MentionedChannels = message.MentionedChannelIds.ToList();
			Reason = FailureReason.NotFailure;
		}

		public string GetSpecifiedArg(string input)
		{
			if (SpecifiedArguments.TryGetValue(input, out string value))
			{
				return value;
			}
			else
			{
				return null;
			}
		}
	}

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

	public struct GuildFileInformation
	{
		public ulong Id { get; }
		public string Name { get; }
		public int MemberCount { get; }

		public GuildFileInformation(ulong id, string name, int memberCount)
		{
			Id = id;
			Name = name;
			MemberCount = memberCount;
		}
	}

	public struct FileInformation
	{
		public FileType FileType { get; }
		public FileInfo FileInfo { get; }

		public FileInformation(FileType fileType, FileInfo fileInfo)
		{
			FileType = fileType;
			FileInfo = fileInfo;
		}
	}

	public struct VerifiedLoggingAction
	{
		public IGuild Guild { get; }
		public IGuildSettings GuildSettings { get; }

		public VerifiedLoggingAction(IGuild guild, IGuildSettings guildSettings)
		{
			Guild = guild;
			GuildSettings = guildSettings;
		}
	}

	public struct LoggedCommand
	{
		public static readonly string _Joiner = Environment.NewLine + new string (' ', 28);
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

	public struct CriticalInformation
	{
		public bool Windows { get; }
		public bool Console { get; }
		public bool FirstInstance { get; }

		public CriticalInformation(bool windows, bool console, bool firstInstance)
		{
			Windows = windows;
			Console = console;
			FirstInstance = firstInstance;
		}
	}
}
