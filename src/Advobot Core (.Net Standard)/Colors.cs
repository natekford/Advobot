using Advobot.Actions;
using Discord;
using System.Collections.Immutable;

namespace Advobot
{
	public static class Colors
	{
		private static ImmutableDictionary<string, Color> _COLORS;
		public static ImmutableDictionary<string, Color> COLORS => _COLORS ?? (_COLORS = GetActions.GetColorDictionary().ToImmutableDictionary());

		public static Color BASE { get; } = new Color(255, 100, 000);
		public static Color JOIN { get; } = new Color(000, 255, 000);
		public static Color LEAV { get; } = new Color(255, 000, 000);
		public static Color UEDT { get; } = new Color(051, 051, 255);
		public static Color ATCH { get; } = new Color(000, 204, 204);
		public static Color MEDT { get; } = new Color(000, 000, 255);
		public static Color MDEL { get; } = new Color(255, 051, 051);
	}
}
