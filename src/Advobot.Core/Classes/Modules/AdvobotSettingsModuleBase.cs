using System.Linq;
using Advobot.Attributes;
using Advobot.Settings;
using Discord.Commands;

namespace Advobot.Classes.Modules
{
	/// <summary>
	/// Handles methods for printing out, modifying, and resetting settings.
	/// </summary>
	/// <typeparam name="TSettings">The settings to modify.</typeparam>
	/// <remarks>
	/// This uses expression a lot so the property name can be gotten from the same argument.
	/// This essentially acts as reflection but with more type safety and resistance to refactoring issues.
	/// </remarks>
	public abstract class ReadOnlyAdvobotSettingsModuleBase<TSettings> : AdvobotModuleBase
		where TSettings : ISettingsBase
	{
		/// <summary>
		/// Returns the settings this module is targetting.
		/// </summary>
		/// <returns></returns>
		protected abstract TSettings Settings { get; }
	}

	/// <summary>
	/// Saves the settings after any command is executed unless <see cref="DontSaveAfterExecutionAttribute"/> is on the command.
	/// </summary>
	/// <typeparam name="TSettings"></typeparam>
	public abstract class AdvobotSettingsModuleBase<TSettings> : ReadOnlyAdvobotSettingsModuleBase<TSettings>
		where TSettings : ISettingsBase
	{
		/// <inheritdoc />
		protected override void AfterExecute(CommandInfo command)
		{
			if (!command.Attributes.Any(x => x is DontSaveAfterExecutionAttribute))
			{
				Settings.Save();
			}
			base.AfterExecute(command);
		}
	}
}