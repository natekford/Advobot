using Discord.WebSocket;

namespace Advobot.Tests.Fakes.Services;

public sealed class FakeConfig : IConfig
{
	public static FakeConfig Singleton { get; } = new();

	public DirectoryInfo BaseBotDirectory { get; } = new(Environment.CurrentDirectory);
	public ulong BotId => ulong.MinValue;
	public string DatabaseConnectionString => "";
	public int Instance => int.MaxValue;
	public int PreviousProcessId => -1;
	public string RestartArguments => "";

	public Task StartAsync(BaseSocketClient client)
		=> Task.CompletedTask;

	public Task<bool> ValidateBotKey(string? key, bool isStartup)
		=> Task.FromResult(true);

	public bool ValidatePath(string? path, bool isStartup)
		=> true;
}