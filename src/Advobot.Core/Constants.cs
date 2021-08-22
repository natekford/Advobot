using System.ComponentModel;
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
		/// Used in AssemblyConfiguration to specify debug mode.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public const string AC_DEB = "Debug";
		/// <summary>
		/// Used in AssemblyConfiguration to specify release mode.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public const string AC_REL = "Release";
		/// <summary>
		/// The emoji to use for an allowed permission. ✅
		/// </summary>
		public const string ALLOWED = "\u2705";
		/// <summary>
		/// Me.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public const string ASSEMBLY_COMPANY = "Advorange";
		/// <summary>
		/// The current year.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public const string ASSEMBLY_COPYRIGHT = "Copyright © 2021";
		/// <summary>
		/// The bot's neutral resources language.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public const string ASSEMBLY_LANGUAGE = "en";
		/// <summary>
		/// The bot's name.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public const string ASSEMBLY_PRODUCT = "Advobot";
		/// <summary>
		/// The bot's version.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public const string ASSEMBLY_VERSION = "3.2.*";
		/// <summary>
		/// The emoji to use for a denied permission. ❌
		/// </summary>
		public const string DENIED = "\u274C";
		/// <summary>
		/// The emoji to use for an inherited permission. ➖
		/// </summary>
		public const string INHERITED = "\u2796";
		/// <summary>
		/// The invite to the Discord server.
		/// </summary>
		/// <remarks>Switched from /xd to this invite since no matter what this inv will link to my server and never someone else's server.</remarks>
		public const string INVITE = "https://discord.gg/MBXypxb";
		/// <summary>
		/// Placeholder prefix for easy replacement.
		/// </summary>
		public const string PREFIX = "%PREFIX%";
		/// <summary>
		/// The repository of the bot.
		/// </summary>
		public const string REPO = "https://github.com/advorange/Advobot";
		/// <summary>
		/// The emoji to use for something unknown. ❔
		/// </summary>
		public const string UNKNOWN = "\u2754";
		/// <summary>
		/// The zero length character to put before every message.
		/// </summary>
		public const string ZERO_WIDTH_SPACE = "\u200b";
		/// <summary>
		/// The bot's version.
		/// </summary>
		public static string BOT_VERSION { get; } = typeof(Constants).Assembly
			.GetName().Version.ToString();
		/// <summary>
		/// The Discord API wrapper version.
		/// </summary>
		public static string DISCORD_NET_VERSION { get; } = typeof(IDiscordClient).Assembly
			.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
			.InformationalVersion;
	}
}