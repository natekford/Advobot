using Advobot.SharedUI.Colors;
using Avalonia.Media;

namespace Advobot.NetCoreUI.Classes.Colors
{
	/// <summary>
	/// Factory for <see cref="SolidColorBrush"/>.
	/// </summary>
	public sealed class NetCoreBrushFactory : BrushFactory<ISolidColorBrush>
	{
		/// <inheritdoc />
		protected override ISolidColorBrush CreateBrush(byte[] bytes)
			=> new SolidColorBrush(Color.FromArgb(bytes[0], bytes[1], bytes[2], bytes[3]));
		/// <inheritdoc />
		protected override byte[] GetBrushBytes(ISolidColorBrush brush)
			=> new[] { brush.Color.A, brush.Color.R, brush.Color.G, brush.Color.B };
	}
}