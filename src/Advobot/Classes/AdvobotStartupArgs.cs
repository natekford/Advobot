using System.Collections.Generic;
using AdvorangesSettingParser;
using AdvorangesUtils;

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
		/// Generates a string which can be passed into the next instance as command line arguments.
		/// </summary>
		/// <param name="previousProcessId"></param>
		/// <param name="currentInstance"></param>
		/// <returns></returns>
		public static string GenerateArgs(int? previousProcessId = null, int? currentInstance = null)
		{
			var parts = new List<string>();
			if (previousProcessId != null)
			{
				parts.Add($"-{nameof(PreviousProcessId)} {previousProcessId}");
			}
			if (currentInstance != null)
			{
				parts.Add($"-{nameof(CurrentInstance)} {currentInstance}");
			}
			return parts.JoinNonNullStrings(" ");
		}
	}
}