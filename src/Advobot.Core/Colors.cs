using Advobot.Core.Actions;
using Discord;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Advobot
{
	public static class Colors
	{
		private static ImmutableDictionary<string, Color> _COLORS;
		public static ImmutableDictionary<string, Color> COLORS => _COLORS ?? (_COLORS = GetColorDictionary());

		//Colors for logging embeds
		public static Color BASE => new Color(255, 100, 000);
		public static Color JOIN => new Color(000, 255, 000);
		public static Color LEAV => new Color(255, 000, 000);
		public static Color UEDT => new Color(051, 051, 255);
		public static Color ATCH => new Color(000, 204, 204);
		public static Color MEDT => new Color(000, 000, 255);
		public static Color MDEL => new Color(255, 051, 051);

		private static ImmutableDictionary<string, Color> GetColorDictionary()
		{
			return typeof(Color).GetFields().Where(x => x.IsPublic).ToDictionary(
				x => x.Name,
				x => (Color)x.GetValue(new Color()),
				StringComparer.OrdinalIgnoreCase).ToImmutableDictionary();
		}
	}
}
