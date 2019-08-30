using Advobot.UI.AbstractUI.Colors;

using Avalonia.Media;

namespace Advobot.UI.Colors
{
	/// <summary>
	/// Factory for <see cref="SolidColorBrush"/>.
	/// </summary>
	public sealed class NetCoreBrushFactory : BrushFactory<ISolidColorBrush>
	{
		/// <inheritdoc />
		public override ISolidColorBrush CreateBrush(byte[] bytes)
			=> new SolidColorBrush(Color.FromArgb(bytes[0], bytes[1], bytes[2], bytes[3]));

		/// <inheritdoc />
		public override byte[] GetBrushBytes(ISolidColorBrush brush)
			=> new[] { brush.Color.A, brush.Color.R, brush.Color.G, brush.Color.B };
	}
}