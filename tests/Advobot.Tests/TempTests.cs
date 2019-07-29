using Advobot.Commands.Standard;
using Advobot.Databases;
using Advobot.Settings;
using AdvorangesUtils;
using Discord.WebSocket;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Advobot.Tests
{
	public sealed class MockLowLevelConfig : ILowLevelConfig
	{
		public ulong BotId => ulong.MinValue;
		public int PreviousProcessId => -1;
		public int CurrentInstance => int.MaxValue;
		public DatabaseType DatabaseType => DatabaseType.LiteDB;
		public string DatabaseConnectionString => "";
		public bool ValidatedPath => true;
		public bool ValidatedKey => true;
		public string RestartArguments => "";
		public DirectoryInfo BaseBotDirectory => null;

		public Task StartAsync(BaseSocketClient client)
			=> Task.CompletedTask;
		public Task<bool> ValidateBotKey(string input, bool startup, Func<BaseSocketClient, IRestartArgumentProvider, Task> restartCallback)
			=> Task.FromResult(true);
		public bool ValidatePath(string input, bool startup)
			=> true;
	}

	public abstract class CommandTester
	{
		public async Task TestCommand()
		{
			var launcher = new AdvobotLauncher(new MockLowLevelConfig());
			await launcher.GetPathAndKeyAsync().CAF();

		}
	}

	[TestClass]
	class TempTests
	{
		[TestMethod]
		public async Task TempTest1_Test()
		{
			//CreateInviteArguments
			var createInviteCommand = new Invites.CreateInvite();
			await createInviteCommand.Command().CAF();
		}
	}
}
