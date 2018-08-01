using Discord.Commands;

namespace Advobot.Classes
{
	/// <summary>
	/// Same as <see cref="NonSavingModuleBase"/> except saves guild settings afterwards.
	/// </summary>
	public abstract class GuildSettingsSavingModuleBase : NonSavingModuleBase
	{
		/// <summary>
		/// Saves the guild settings.
		/// </summary>
		/// <param name="command"></param>
		protected override void AfterExecute(CommandInfo command)
		{
			Context.GuildSettings.SaveSettings();
			base.AfterExecute(command);
		}
	}
}
