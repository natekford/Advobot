using System;
using System.Threading.Tasks;

using Advobot.Services.GuildSettings;

using Discord;

namespace Advobot.Tests.Fakes.Services.GuildSettings
{
	public sealed class FakeGuildSettingsFactory : IGuildSettingsFactory
	{
		private readonly IGuildSettings _Settings;

		public FakeGuildSettingsFactory(IGuildSettings settings)
		{
			_Settings = settings;
		}

		public Type GuildSettingsType => _Settings.GetType();

		public Task<IGuildSettings> GetOrCreateAsync(IGuild guild)
			=> Task.FromResult(_Settings);

		public Task RemoveAsync(ulong guildId) => throw new NotImplementedException();

		public bool TryGet(ulong guildId, out IGuildSettings settings) => throw new NotImplementedException();
	}
}