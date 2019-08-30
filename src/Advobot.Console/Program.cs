using System.Threading.Tasks;

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
			System.Console.Title = "Advobot";
			await AdvobotLauncher.NoConfigurationStart(args).CAF();
			await Task.Delay(-1).CAF();
		}
	}
}