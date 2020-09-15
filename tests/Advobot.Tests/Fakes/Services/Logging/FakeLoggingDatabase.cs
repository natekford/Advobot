using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.Logging;
using Advobot.Logging.Database;
using Advobot.Logging.Models;
using Advobot.Logging.ReadOnlyModels;

namespace Advobot.Tests.Fakes.Services.Logging
{
	public sealed class FakeLoggingDatabase : ILoggingDatabase
	{
		private readonly LogChannels _LogChannels;

		public FakeLoggingDatabase(LogChannels channels)
		{
			_LogChannels = channels;
		}

		public Task<int> AddIgnoredChannelsAsync(ulong guildId, IEnumerable<ulong> channels) => throw new NotImplementedException();

		public Task<int> AddLogActionsAsync(ulong guildId, IEnumerable<LogAction> actions) => throw new NotImplementedException();

		public Task<int> DeleteIgnoredChannelsAsync(ulong guildId, IEnumerable<ulong> channels) => throw new NotImplementedException();

		public Task<int> DeleteLogActionsAsync(ulong guildId, IEnumerable<LogAction> actions) => throw new NotImplementedException();

		public Task<IReadOnlyList<ulong>> GetIgnoredChannelsAsync(ulong guildId) => throw new NotImplementedException();

		public Task<IReadOnlyList<LogAction>> GetLogActionsAsync(ulong guildId) => throw new NotImplementedException();

		public Task<IReadOnlyLogChannels> GetLogChannelsAsync(ulong guildId)
			=> Task.FromResult<IReadOnlyLogChannels>(_LogChannels);

		public Task<int> UpsertLogChannelAsync(Log log, ulong guildId, ulong? channelId) => throw new NotImplementedException();
	}
}