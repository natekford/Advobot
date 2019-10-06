using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.Logging.Models;
using Advobot.Logging.ReadOnlyModels;
using Advobot.Logging.Service;
using Advobot.Services.GuildSettings.Settings;

namespace Advobot.Tests.Fakes.Services.Logging
{
	public sealed class FakeLoggingService : ILoggingService
	{
		private readonly LogChannels _LogChannels;

		public FakeLoggingService(LogChannels channels)
		{
			_LogChannels = channels;
		}

		public Task AddIgnoredChannelsAsync(ulong guildId, IEnumerable<ulong> channels) => throw new NotImplementedException();

		public Task AddLogActionsAsync(ulong guildId, IEnumerable<LogAction> actions) => throw new NotImplementedException();

		public Task<IReadOnlyList<ulong>> GetIgnoredChannelsAsync(ulong guildId) => throw new NotImplementedException();

		public Task<IReadOnlyList<LogAction>> GetLogActionsAsync(ulong guildId) => throw new NotImplementedException();

		public Task<IReadOnlyLogChannels> GetLogChannelsAsync(ulong guildId)
			=> Task.FromResult<IReadOnlyLogChannels>(_LogChannels);

		public Task RemoveIgnoredChannelsAsync(ulong guildId, IEnumerable<ulong> channels) => throw new NotImplementedException();

		public Task RemoveImageLogChannelAsync(ulong guildId) => throw new NotImplementedException();

		public Task RemoveLogActionsAsync(ulong guildId, IEnumerable<LogAction> actions) => throw new NotImplementedException();

		public Task RemoveModLogChannelAsync(ulong guildId) => throw new NotImplementedException();

		public Task RemoveServerLogChannelAsync(ulong guildId) => throw new NotImplementedException();

		public Task UpdateImageLogChannelAsync(ulong guildId, ulong channelId) => throw new NotImplementedException();

		public Task UpdateModLogChannelAsync(ulong guildId, ulong channelId) => throw new NotImplementedException();

		public Task UpdateServerLogChannelAsync(ulong guildId, ulong channelId) => throw new NotImplementedException();
	}
}