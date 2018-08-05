using System.Diagnostics;
using AdvorangesSettingParser;

namespace Advobot.Classes
{
	/// <summary>
	/// Arguments intended to be parsed from the command line args.
	/// </summary>
	public sealed class AdvobotStartupArgs
	{
		/// <summary>
		/// The previous process id of the application. This is used with the .Net Framework version to make sure the old one is killed first when restarting.
		/// </summary>
		public int PreviousProcessId { get; private set; } = -1;
		/// <summary>
		/// The current instance number of the application. This is used to find the correct config.
		/// </summary>
		public int CurrentInstance { get; private set; } = -1;

		/// <summary>
		/// Creates an instance of <see cref="AdvobotStartupArgs"/>.
		/// </summary>
		public AdvobotStartupArgs(string[] args)
		{
			//No help command because this is not intended to be used more than semi internally
			new SettingParser(false, "-", "--", "/")
			{
				//Don't bother adding descriptions because of the aforementioned removal
				new Setting<int>(new[] { nameof(PreviousProcessId), "procid" }, x => PreviousProcessId = x),
				new Setting<int>(new[] { nameof(CurrentInstance), "instance" }, x => CurrentInstance = x)
			}.Parse(args);
		}

		/// <summary>
		/// Creates low level config using the passed in startup args.
		/// </summary>
		/// <returns></returns>
		public LowLevelConfig CreateConfig()
		{
			return LowLevelConfig.Load(CurrentInstance);
		}
		/// <summary>
		/// Generates a string which can be passed into the next instance as command line arguments.
		/// </summary>
		/// <param name="config"></param>
		/// <returns></returns>
		public static string GenerateArgs(LowLevelConfig config)
		{
			return $"-{nameof(PreviousProcessId)} {Process.GetCurrentProcess().Id} " +
				$"-{nameof(CurrentInstance)} {config.InstanceNumber}";
		}
	}
}