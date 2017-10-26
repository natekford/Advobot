using Advobot.UILauncher.Actions;
using Advobot.UILauncher.Enums;
using Newtonsoft.Json;
using System.Windows.Media;

namespace Advobot.UILauncher.Classes
{
	internal struct BrushTargetAndValue
	{
		[JsonProperty]
		public ColorTarget Target { get; private set; }
		[JsonProperty]
		public Brush Brush { get; private set; }

		public BrushTargetAndValue(ColorTarget target, string colorString)
		{
			Target = target;
			Brush = UIModification.MakeSolidColorBrush(colorString);
		}
	}
}
