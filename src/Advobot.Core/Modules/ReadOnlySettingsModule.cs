using Advobot.Settings;

namespace Advobot.Modules
{
	/// <summary>
	/// Handles methods for printing out, modifying, and resetting settings.
	/// </summary>
	/// <typeparam name="TSettings">The settings to modify.</typeparam>
	/// <remarks>
	/// This uses expression a lot so the property name can be gotten from the same argument.
	/// This essentially acts as reflection but with more type safety and resistance to refactoring issues.
	/// </remarks>
	public abstract class ReadOnlySettingsModule<TSettings> : AdvobotModuleBase
		where TSettings : ISettingsBase
	{
		/// <summary>
		/// Returns the settings this module is targetting.
		/// </summary>
		/// <returns></returns>
		protected abstract TSettings Settings { get; }
	}
}