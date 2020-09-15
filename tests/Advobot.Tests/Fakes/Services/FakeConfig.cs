using System;
using System.IO;
using System.Threading.Tasks;

using Advobot.Settings;

using Discord.WebSocket;

namespace Advobot.Tests.Fakes.Services
{
	public sealed class FakeConfig : IConfig
	{
		public DirectoryInfo BaseBotDirectory => throw new NotImplementedException();
		public ulong BotId => ulong.MinValue;
		public string DatabaseConnectionString => "";
		public int Instance => int.MaxValue;
		public int PreviousProcessId => -1;
		public string RestartArguments => "";

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