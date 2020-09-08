using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using Advobot.Logging.Service;

using AdvorangesUtils;

using Discord;

namespace Advobot.Logging.Context
{
	public sealed class LoggingHandler<T> where T : ILoggingState
	{
		public LogAction Action { get; }
		public IReadOnlyList<Func<ILoggingContext<T>, Task>> Actions { get; set; } = Array.Empty<Func<ILoggingContext<T>, Task>>();
		public ILoggingService Service { get; }

		public LoggingHandler(LogAction action, ILoggingService service)
		{
			Service = service;
			Action = action;
		}

		[return: MaybeNull]
		public static async Task<ILoggingContext<T>> CreateContextAsync(
			ILoggingService service,
			T state)
		{
			if (!(state.Guild is IGuild guild))
			{
				return null!;
			}

			var channels = await service.GetLogChannelsAsync(guild.Id).CAF();
			var imageLog = await guild.GetTextChannelAsync(channels.ImageLogId).CAF();
			var modLog = await guild.GetTextChannelAsync(channels.ModLogId).CAF();
			var serverLog = await guild.GetTextChannelAsync(channels.ServerLogId).CAF();
			var actions = await service.GetLogActionsAsync(guild.Id).CAF();
			var bot = await guild.GetCurrentUserAsync().CAF();
			return new PrivateLoggingContext(
				state, guild, bot,
				imageLog, modLog, serverLog, actions);
		}

		public async Task HandleAsync(T state)
		{
			if (!state.IsValid)
			{
				return;
			}

			var context = await CreateContextAsync(Service, state).CAF();
			if (context?.Actions.Contains(Action) != true)
			{
				return;
			}

			var canLog = await state.CanLog(Service, context).CAF();
			if (!canLog)
			{
				return;
			}

			foreach (var task in Actions)
			{
				try
				{
					await task.Invoke(context).CAF();
				}
				catch (Exception e)
				{
					e.Write();
				}
			}
		}

		private sealed class PrivateLoggingContext : ILoggingContext<T>
		{
			public IReadOnlyList<LogAction> Actions { get; }
			public IGuildUser Bot { get; }
			public IGuild Guild { get; }
			public ITextChannel? ImageLog { get; }
			public ITextChannel? ModLog { get; }
			public ITextChannel? ServerLog { get; }
			public T State { get; }

			public PrivateLoggingContext(
				T state,
				IGuild guild,
				IGuildUser bot,
				ITextChannel? imageLog,
				ITextChannel? modLog,
				ITextChannel? serverLog,
				IReadOnlyList<LogAction> actions)
			{
				State = state;
				Guild = guild;
				Bot = bot;
				ImageLog = imageLog;
				ModLog = modLog;
				ServerLog = serverLog;
				Actions = actions;
			}
		}
	}
}