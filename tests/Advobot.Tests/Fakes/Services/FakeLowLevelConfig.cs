using System;
using System.IO;
using System.Threading.Tasks;

using Advobot.Databases;
using Advobot.Settings;

using Discord.WebSocket;

namespace Advobot.Tests.Fakes.Services
{
	public sealed class FakeLowLevelConfig : ILowLevelConfig
	{
		public DirectoryInfo BaseBotDirectory => throw new NotImplementedException();
		public ulong BotId => ulong.MinValue;
		public int CurrentInstance => int.MaxValue;
		public string DatabaseConnectionString => "";
		public DatabaseType DatabaseType => DatabaseType.LiteDB;
		public int PreviousProcessId => -1;
		public string RestartArguments => "";
		public bool ValidatedKey => true;
		public bool ValidatedPath => true;

		public Task StartAsync(BaseSocketClient client)
			=> Task.CompletedTask;

		public Task<bool> ValidateBotKey(
			string? input,
			bool startup,
			Func<BaseSocketClient, IRestartArgumentProvider, Task> restartCallback)
			=> Task.FromResult(true);

		public bool ValidatePath(string? input, bool startup)
			=> true;
	}
}