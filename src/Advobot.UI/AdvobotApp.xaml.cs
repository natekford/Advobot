using Avalonia;
using Avalonia.Markup.Xaml;

namespace Advobot.UI;

public class AdvobotApp : Application
{
	public override void Initialize()
		=> AvaloniaXamlLoader.Load(this);
}