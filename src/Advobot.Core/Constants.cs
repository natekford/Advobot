using System.Reflection;

using Discord;

namespace Advobot
{
	/// <summary>
	/// Global values expected to stay the same.
	/// </summary>
	public static class Constants
	{
		/// <summary>
		/// The emoji to use for an allowed permission. ✅
		/// </summary>
		public const string ALLOWED = "\u2705";

		/// <summary>
		/// The bot's version.
		/// </summary>
		public const string BOT_VERSION = Version.VERSION_NUMBER;

		/// <summary>
		/// The emoji to use for a denied permission. ❌
		/// </summary>
		public const string DENIED = "\u274C";

		/// <summary>
		/// The invite to the Discord server.
		/// </summary>
		/// <remarks>Switched from /xd to this invite since no matter what this inv will link to my server and never someone else's server.</remarks>
		public const string DISCORD_INV = "https://discord.gg/MBXypxb";

		/// <summary>
		/// Placeholder prefix for easy replacement.
		/// </summary>
		public const string PREFIX = "%PREFIX%";

		/// <summary>
		/// The repository of the bot.
		/// </summary>
		public const string REPO = "https://github.com/advorange/Advobot";

		/// <summary>
		/// The schema
		/// </summary>
		public const int SCHEMA_VERSION = 3;

		/// <summary>
		/// The zero length character to put before every message.
		/// </summary>
		public const string ZERO_LENGTH_CHAR = "\u200b";

		/// <summary>
		/// The Discord api wrapper version.
		/// </summary>
		public static readonly string API_VERSION = Assembly.GetAssembly(typeof(IDiscordClient)).GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
	}
}