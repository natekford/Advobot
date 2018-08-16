using Avalonia;
using Avalonia.Markup.Xaml;

namespace Advobot.NetCoreUI
{
	public class AdvobotNetCoreApp : Application
	{
		public override void Initialize()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}
