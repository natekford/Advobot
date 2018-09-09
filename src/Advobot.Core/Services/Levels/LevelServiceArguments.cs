using System;

namespace Advobot.Services.Levels
{
	/// <summary>
	/// Arguments used when creating <see cref="LevelService"/>.
	/// </summary>
	public sealed class LevelServiceArguments
	{
		/// <summary>
		/// Part of the level formula. Bigger means levels get lower.
		/// </summary>
		public double Log { get; set; } = 9;
		/// <summary>
		/// Part of the level formula. Bigger means levels get higher.
		/// </summary>
		public double Pow { get; set; } = 2.3;
		/// <summary>
		/// How long to wait between counting xp from users.
		/// </summary>
		public TimeSpan Time { get; set; } = TimeSpan.FromSeconds(5);
		/// <summary>
		/// The base experience from each message.
		/// </summary>
		public int BaseExperience { get; set; } = 25;
	}
}