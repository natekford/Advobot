using System;

using Discord;

namespace Advobot.Gacha
{
	//TODO: put into settings class
	public static class GachaConstants
	{
		public static readonly int CharactersPerPage = 15;
		public static readonly Color Claimed = new(128, 0, 32);
		public static readonly TimeSpan ClaimLength = TimeSpan.FromSeconds(15);
		public static readonly Color Unclaimed = new(244, 174, 66);
		public static readonly Color Wished = new(50, 205, 50);
	}
}