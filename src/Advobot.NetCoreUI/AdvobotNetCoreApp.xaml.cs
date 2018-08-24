using Avalonia;
using Avalonia.Markup.Xaml;

namespace Advobot.NetCoreUI
{
	public class AdvobotNetCoreApp : Application
	{
		static AdvobotNetCoreApp()
		{
			var t = Advobot.NetCoreUI.Classes.ResizableFont.FontResizeProperty;
		}

		public override void Initialize()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}
