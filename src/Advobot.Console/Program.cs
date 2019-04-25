using System.Threading.Tasks;
using Advobot.Utilities;
using AdvorangesUtils;

namespace Advobot.Console
{
	/// <summary>
	/// Starting point for Advobot in a console.
	/// </summary>
	public sealed class ConsoleLauncher
	{
		private static async Task Main(string[] args)
		{
			var launcher = new AdvobotConsoleLauncher(args);
			await launcher.GetPathAndKeyAsync().CAF();
			var services = launcher.GetDefaultServices(DiscordUtils.GetCommandAssemblies());
			var provider = launcher.CreateProvider(services);
			await launcher.StartAsync(provider).CAF();
			await Task.Delay(-1).CAF();
		}
	}
}