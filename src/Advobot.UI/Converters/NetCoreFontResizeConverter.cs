using Advobot.UI.AbstractUI.Converters;

using Avalonia.Data.Converters;

namespace Advobot.UI.Converters;

/// <summary>
/// Resizes text.
/// </summary>
public sealed class NetCoreFontResizeConverter : FontResizeConverter, IValueConverter
{
	/// <summary>
	/// Creates an instance of <see cref="NetCoreFontResizeConverter"/>.
	/// </summary>
	public NetCoreFontResizeConverter() : base(.015) { }

	/// <summary>
	/// Creates an instance of <see cref="NetCoreFontResizeConverter"/>.
	/// </summary>
	/// <param name="convertFactor"></param>
	public NetCoreFontResizeConverter(double convertFactor) : base(convertFactor) { }
}