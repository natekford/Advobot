using Discord.Commands;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Same as <see cref="AdvobotModuleBase"/> except saves guild settings afterwards.
	/// </summary>
	public class SavingModuleBase : AdvobotModuleBase
	{
		protected override void AfterExecute(CommandInfo command)
		{
			Context.GuildSettings.SaveSettings();
			base.AfterExecute(command);
		}
	}
}
