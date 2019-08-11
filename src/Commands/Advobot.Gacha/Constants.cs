using System;
using Discord;

namespace Advobot.Gacha
{
	public static class Constants
	{
		public static readonly string Heart = "claim";
		public static readonly string Left = "left";
		public static readonly string Right = "right";
		public static readonly string Confirm = "confirm";
		public static readonly string Deny = "cancel";

		public static readonly Color Unclaimed = new Color(244, 174, 66);
		public static readonly Color Claimed = new Color(128, 0, 32);
		public static readonly Color Wished = new Color(50, 205, 50);

		public static readonly TimeSpan ClaimLength = TimeSpan.FromSeconds(15);

		public static readonly int CharactersPerPage = 15;
	}
}
