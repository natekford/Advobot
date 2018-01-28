using Discord.Commands;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Same as <see cref="NonSavingModuleBase"/> except saves guild settings afterwards.
	/// </summary>
	public class SavingModuleBase : NonSavingModuleBase
	{
		protected override void AfterExecute(CommandInfo command)
		{
			Context.GuildSettings.SaveSettings();
			base.AfterExecute(command);
		}
	}
}
