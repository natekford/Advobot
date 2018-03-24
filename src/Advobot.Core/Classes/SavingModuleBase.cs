using Discord.Commands;

namespace Advobot.Core.Classes
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
