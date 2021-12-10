using Advobot.Settings;
using Advobot.UI.AbstractUI.Colors;

using System.ComponentModel;

namespace Advobot.UI.Colors;

/// <summary>
/// Settings for the UI.
/// </summary>
/// <typeparam name="TBrush"></typeparam>
public interface IColorSettings<TBrush> : ISavable, INotifyPropertyChanged
{
	/// <summary>
	/// The active theme in the bot UI.
	/// </summary>
	ColorTheme ActiveTheme { get; set; }

	/// <summary>
	/// The user defined colors for <see cref="ColorTheme"/>.
	/// </summary>
	ITheme<TBrush> UserDefinedColors { get; }
}