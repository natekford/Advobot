using System.Linq;
using Advobot.Attributes;
using Advobot.Settings;
using Discord.Commands;

namespace Advobot.Modules
{
	/// <summary>
	/// Saves the settings after any command is executed unless <see cref="DontSaveAfterExecutionAttribute"/> is on the command.
	/// </summary>
	/// <typeparam name="TSettings"></typeparam>
	public abstract class SettingsModule<TSettings> : ReadOnlySettingsModule<TSettings>
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