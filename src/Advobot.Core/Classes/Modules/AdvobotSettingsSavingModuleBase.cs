using Advobot.Interfaces;
using Discord.Commands;

namespace Advobot.Classes.Modules
{
	/// <summary>
	/// Handles saving settings in addition to other things.
	/// </summary>
	/// <typeparam name="TSettings"></typeparam>
	public abstract class AdvobotSettingsSavingModuleBase<TSettings> : AdvobotSettingsModuleBase<TSettings> where TSettings : ISettingsBase
	{
		/// <summary>
		/// Saves the settings then calls the base <see cref="ModuleBase{T}.AfterExecute(CommandInfo)"/>.
		/// </summary>
		/// <param name="command"></param>
		protected override void AfterExecute(CommandInfo command)
		{
			Settings.SaveSettings(BotSettings);
			base.AfterExecute(command);
		}
	}
}