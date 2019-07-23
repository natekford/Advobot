using Discord;
using System;

namespace Advobot.Gacha
{
	public static class Constants
	{
		public static readonly Emoji Heart = new Emoji("\u2764"); //❤
		public static readonly Emoji Left = new Emoji("\u25C0"); //◀
		public static readonly Emoji DoubleLeft = new Emoji("\u2B05"); //⬅
		public static readonly Emoji Right = new Emoji("\u25B6"); //▶
		public static readonly Emoji DoubleRight = new Emoji("\u27A1"); //➡
		public static readonly Emoji Confirm = new Emoji("\u2705"); //✅
		public static readonly Emoji Deny = new Emoji("\u274C"); //❌

		public static readonly Color Unclaimed = new Color(244, 174, 66);
		public static readonly Color Claimed = new Color(128, 0, 32);
		public static readonly Color Wished = new Color(50, 205, 50);

		public static readonly TimeSpan ClaimLength = TimeSpan.FromSeconds(15);
	}
}
