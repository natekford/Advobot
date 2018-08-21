using Advobot.SharedUI.Converters;
using System.Windows.Data;

namespace Advobot.NetFrameworkUI.Classes.Converters
{
	/// <summary>
	/// Resizes text.
	/// </summary>
	public sealed class NetFrameworkFontResizeConverter : FontResizeConverter, IValueConverter
	{
		/// <summary>
		/// Creates an instance of <see cref="NetFrameworkFontResizeConverter"/>.
		/// </summary>
		/// <param name="convertFactor"></param>
		public NetFrameworkFontResizeConverter(double convertFactor) : base(convertFactor) { }
	}
}
