
using Advobot.Logging;
using Advobot.Logging.Database;
using Advobot.Logging.Models;

namespace Advobot.Tests.Fakes.Services.Logging
{
	public sealed class FakeLoggingDatabase : ILoggingDatabase
	{
		private readonly Dictionary<ulong, LogChannels> _LogChannels = new();

		public Task<int> AddIgnoredChannelsAsync(ulong guildId, IEnumerable<ulong> channels) => throw new NotImplementedException();

		public Task<int> AddLogActionsAsync(ulong guildId, IEnumerable<LogAction> actions) => throw new NotImplementedException();

		public Task<int> DeleteIgnoredChannelsAsync(ulong guildId, IEnumerable<ulong> channels) => throw new NotImplementedException();

		public Task<int> DeleteLogActionsAsync(ulong guildId, IEnumerable<LogAction> actions) => throw new NotImplementedException();

		public Task<IReadOnlyList<ulong>> GetIgnoredChannelsAsync(ulong guildId) => throw new NotImplementedException();

		public Task<IReadOnlyList<LogAction>> GetLogActionsAsync(ulong guildId) => throw new NotImplementedException();

		public Task<LogChannels> GetLogChannelsAsync(ulong guildId)
			=> Task.FromResult(_LogChannels.TryGetValue(guildId, out var current) ? current : new LogChannels());

		public Task<int> UpsertLogChannelAsync(Log log, ulong guildId, ulong? channelId)
		{
			if (!_LogChannels.TryGetValue(guildId, out var current))
			{
				current = new LogChannels();
			}

			_LogChannels[guildId] = current with
			{
				ImageLogId = log == Log.Image ? channelId ?? 0 : current.ImageLogId,
				ModLogId = log == Log.Mod ? channelId ?? 0 : current.ModLogId,
				ServerLogId = log == Log.Server ? channelId ?? 0 : current.ServerLogId,
			};
			return Task.FromResult(1);
		}
	}
}