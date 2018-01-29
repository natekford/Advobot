using Discord.Commands;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Same as <see cref="NonSavingModuleBase"/> except saves guild settings afterwards.
	/// </summary>
	public class GuildSettingsSavingModuleBase : NonSavingModuleBase
	{
		protected override void AfterExecute(CommandInfo command)
		{
			Context.GuildSettings.SaveSettings();
			base.AfterExecute(command);
		}
	}

	/// <summary>
	/// Same as <see cref="NonSavingModuleBase"/> except saves bot settings afterwards.
	/// </summary>
	public class BotSettingsSavingModuleBase : NonSavingModuleBase
	{
		protected override void AfterExecute(CommandInfo command)
		{
			Context.BotSettings.SaveSettings();
			base.AfterExecute(command);
		}
	}
}
