using Discord.Commands;

namespace Advobot.Classes
{
	/// <summary>
	/// Same as <see cref="NonSavingModuleBase"/> except saves bot settings afterwards.
	/// </summary>
	public abstract class BotSettingsSavingModuleBase : NonSavingModuleBase
	{
		/// <summary>
		/// Saves the bot settings.
		/// </summary>
		/// <param name="command"></param>
		protected override void AfterExecute(CommandInfo command)
		{
			Context.BotSettings.SaveSettings();
			base.AfterExecute(command);
		}
	}
}