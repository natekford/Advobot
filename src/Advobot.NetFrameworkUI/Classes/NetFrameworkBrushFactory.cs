using System.Windows.Media;
using Advobot.SharedUI;

namespace Advobot.NetFrameworkUI.Classes
{
	/// <summary>
	/// Factory for <see cref="SolidColorBrush"/>.
	/// </summary>
	public sealed class NetFrameworkBrushFactory : BrushFactory<SolidColorBrush>
	{
		/// <inheritdoc />
		protected override SolidColorBrush CreateBrush(byte[] bytes)
		{
			return new SolidColorBrush(Color.FromArgb(bytes[0], bytes[1], bytes[2], bytes[3]));
		}
	}
}