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
			var launcher = new AdvobotNetCoreLauncher(args);
			launcher.SetPath();
			await launcher.SetBotKey().CAF();
			await launcher.Start().CAF();
		}
	}
}