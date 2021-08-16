using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Advobot.Logging.Database;

using AdvorangesUtils;

using Discord;

namespace Advobot.Logging.Context
{
	public sealed class LogHandler<T> : ICollection<Func<ILogContext<T>, Task>>
		where T : ILogState
	{
		private readonly ICollection<Func<ILogContext<T>, Task>> _Actions =
			new List<Func<ILogContext<T>, Task>>();

		public LogAction Action { get; }
		public ILoggingDatabase Db { get; }
		public int Count => _Actions.Count;
		public bool IsReadOnly => _Actions.IsReadOnly;

		public LogHandler(LogAction action, ILoggingDatabase db)
		{
			Db = db;
			Action = action;
		}

		public void Add(Func<ILogContext<T>, Task> item)
			=> _Actions.Add(item);

		public void Clear()
			=> _Actions.Clear();

		public bool Contains(Func<ILogContext<T>, Task> item)
			=> _Actions.Contains(item);

		public void CopyTo(Func<ILogContext<T>, Task>[] array, int arrayIndex)
			=> _Actions.CopyTo(array, arrayIndex);

		public IEnumerator<Func<ILogContext<T>, Task>> GetEnumerator()
			=> _Actions.GetEnumerator();

		public async Task HandleAsync(T state)
		{
			var context = await CreateContextAsync(state).CAF();
			if (context is null)
			{
				return;
			}

			var canLog = await state.CanLog(Db, context).CAF();
			if (!canLog)
			{
				return;
			}

			// Run logging in background since the results do not matter
			_ = Task.Run(async () =>
			{
				foreach (var action in this)
				{
					try
					{
						await action.Invoke(context).CAF();
					}
					catch (Exception e)
					{
						e.Write();
					}
				}
			});
		}

		public bool Remove(Func<ILogContext<T>, Task> item)
			=> _Actions.Remove(item);

		IEnumerator IEnumerable.GetEnumerator()
			=> ((IEnumerable)_Actions).GetEnumerator();

		private async Task<LogContext?> CreateContextAsync(T state)
		{
			// Invalid state or state somehow doesn't have a guild
			if (!state.IsValid || state.Guild is not IGuild guild)
			{
				return null;
			}

			// Action is disabled so don't bother logging
			var actions = await Db.GetLogActionsAsync(guild.Id).CAF();
			if (!actions.Contains(Action))
			{
				return null;
			}

			var channels = await Db.GetLogChannelsAsync(guild.Id).CAF();
			var imageLog = await guild.GetTextChannelAsync(channels.ImageLogId).CAF();
			var modLog = await guild.GetTextChannelAsync(channels.ModLogId).CAF();
			var serverLog = await guild.GetTextChannelAsync(channels.ServerLogId).CAF();
			// No log channels so there's no point in going further
			if (imageLog is null && modLog is null && serverLog is null)
			{
				return null;
			}

			var bot = await guild.GetCurrentUserAsync().CAF();
			return new(state, guild, bot, imageLog, modLog, serverLog);
		}

		private sealed class LogContext : ILogContext<T>
		{
			public IGuildUser Bot { get; }
			public IGuild Guild { get; }
			public ITextChannel? ImageLog { get; }
			public ITextChannel? ModLog { get; }
			public ITextChannel? ServerLog { get; }
			public T State { get; }

			public LogContext(
				T state,
				IGuild guild,
				IGuildUser bot,
				ITextChannel? imageLog,
				ITextChannel? modLog,
				ITextChannel? serverLog)
			{
				State = state;
				Guild = guild;
				Bot = bot;
				ImageLog = imageLog;
				ModLog = modLog;
				ServerLog = serverLog;
			}
		}
	}
}