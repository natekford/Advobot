using Advobot.Interfaces;

namespace Advobot.SharedUI.Colors
{
	/// <summary>
	/// Settings for the UI.
	/// </summary>
	/// <typeparam name="TBrush"></typeparam>
	public interface IColorSettings<TBrush> : ISettingsBase
	{
		/// <summary>
		/// The active theme in the bot UI.
		/// </summary>
		ColorTheme Theme { get; set; }
		/// <summary>
		/// The user defined colors for <see cref="ColorTheme"/>.
		/// </summary>
		ITheme<TBrush> UserDefinedColors { get; }
	}
}