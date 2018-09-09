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
			await launcher.GetPathAndKey().CAF();
			var services = launcher.GetDefaultServices(DiscordUtils.GetCommandAssemblies());
			var provider = services.CreateProvider();
			await launcher.Start(provider).CAF();
			await Task.Delay(-1).CAF();
		}
	}
}