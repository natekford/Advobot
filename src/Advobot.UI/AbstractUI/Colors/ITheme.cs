using System.ComponentModel;

namespace Advobot.UI.AbstractUI.Colors;

/// <summary>
/// Holds a collection of colors for usage in UI.
/// </summary>
/// <typeparam name="TBrush"></typeparam>
public interface ITheme<TBrush> : IDictionary<string, TBrush>, INotifyPropertyChanged
{
	/// <summary>
	/// Makes this theme unable to be edited.
	/// </summary>
	void Freeze();
}