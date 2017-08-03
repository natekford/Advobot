using Advobot.Actions;
using Advobot.Enums;
using Advobot.Interfaces;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Advobot
{
	namespace Structs
	{
		public struct BotGuildPermission : IPermission
		{
			public string Name { get; }
			public ulong Bit { get; }

			public BotGuildPermission(string name, int position)
			{
				Name = name;
				Bit = (1U << position);
			}
		}

		public struct BotChannelPermission : IPermission
		{
			public string Name { get; }
			public ulong Bit { get; }
			public bool General { get; }
			public bool Text { get; }
			public bool Voice { get; }

			public BotChannelPermission(string name, int position, bool gen = false, bool text = false, bool voice = false)
			{
				Name = name;
				Bit = (1U << position);
				General = gen;
				Text = text;
				Voice = voice;
			}
		}

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
			public ITextChannel LoggingChannel { get; }

			public VerifiedLoggingAction(IGuild guild, IGuildSettings guildSettings, ITextChannel loggingChannel)
			{
				Guild = guild;
				GuildSettings = guildSettings;
				LoggingChannel = loggingChannel;
			}
		}

		public struct LoggedCommand
		{
			public string Guild { get; }
			public string Channel { get; }
			public string User { get; }
			public string Time { get; }
			public string Text { get; }

			public LoggedCommand(ICommandContext context)
			{
				Guild = context.Guild.FormatGuild();
				Channel = context.Channel.FormatChannel();
				User = context.User.FormatUser();
				Time = FormattingActions.FormatDateTime(context.Message.CreatedAt);
				Text = context.Message.Content;
			}

			public override string ToString()
			{
				var guild = String.Format("Guild: {0}", Guild);
				var channel = String.Format("Channel: {0}", Channel);
				var user = String.Format("User: {0}", User);
				var time = String.Format("Time: {0}", Time);
				var text = String.Format("Text: {0}", Text);
				return String.Join(Environment.NewLine + new string(' ', 25), new[] { guild, channel, user, time, text });
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
}
