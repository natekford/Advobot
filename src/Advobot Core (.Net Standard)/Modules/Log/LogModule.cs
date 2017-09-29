using Advobot.Actions;
using Advobot.Actions.Formatting;
using Advobot.Classes;
using Advobot.Interfaces;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Advobot.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Modules.Log
{
	/// <summary>
	/// Logs certain events.
	/// </summary>
	/// <remarks>
	/// This is probably the second worst part of the bot, right behind the UI. Slightly ahead of saving settings though.
	/// </remarks>
	internal sealed class Logging : ILogModule
	{
		private LogCounter[] _LoggedCommands;
		private LogCounter[] _LoggedUserActions;
		private LogCounter[] _LoggedMessageActions;
		private LogCounter[] _LoggedAttachments;

		public List<LoggedCommand> RanCommands	{ get; private set; } = new List<LoggedCommand>();
		public LogCounter TotalUsers			{ get; private set; } = new LogCounter();
		public LogCounter TotalGuilds			{ get; private set; } = new LogCounter();
		public LogCounter AttemptedCommands		{ get; private set; } = new LogCounter();
		public LogCounter SuccessfulCommands	{ get; private set; } = new LogCounter();
		public LogCounter FailedCommands		{ get; private set; } = new LogCounter();
		public LogCounter UserJoins				{ get; private set; } = new LogCounter();
		public LogCounter UserLeaves			{ get; private set; } = new LogCounter();
		public LogCounter UserChanges			{ get; private set; } = new LogCounter();
		public LogCounter MessageEdits			{ get; private set; } = new LogCounter();
		public LogCounter MessageDeletes		{ get; private set; } = new LogCounter();
		public LogCounter Messages				{ get; private set; } = new LogCounter();
		public LogCounter Images				{ get; private set; } = new LogCounter();
		public LogCounter Gifs					{ get; private set; } = new LogCounter();
		public LogCounter Files					{ get; private set; } = new LogCounter();

		public IBotLogger BotLogger				{ get; private set; }
		public IGuildLogger GuildLogger			{ get; private set; }
		public IUserLogger UserLogger			{ get; private set; }
		public IMessageLogger MessageLogger		{ get; private set; }

		public Logging(IServiceProvider provider)
		{
			_LoggedCommands			= new[] { AttemptedCommands, SuccessfulCommands, FailedCommands };
			_LoggedUserActions		= new[] { UserJoins, UserLeaves, UserChanges };
			_LoggedMessageActions	= new[] { MessageEdits, MessageDeletes };
			_LoggedAttachments		= new[] { Images, Gifs, Files };

			BotLogger				= new BotLogger(this, provider);
			GuildLogger				= new GuildLogger(this, provider);
			UserLogger				= new UserLogger(this, provider);
			MessageLogger			= new MessageLogger(this, provider);
		}

		public string FormatLoggedCommands()
		{
			return LogCounter.FormatMultiple(true, _LoggedCommands);
		}
		public string FormatLoggedActions()
		{
			return LogCounter.FormatMultiple(true, _LoggedUserActions) +
				LogCounter.FormatMultiple(true, _LoggedMessageActions) +
				LogCounter.FormatMultiple(true, _LoggedAttachments);
		}
	}
}