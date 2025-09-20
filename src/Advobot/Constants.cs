using Discord;

using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace Advobot;

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
	/// Me.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public const string ASSEMBLY_COMPANY = "Advorange";
	/// <summary>
	/// Used in AssemblyConfiguration to specify debug or release mode.
	/// </summary>
	/// <remarks>
	/// Since all the command projects are in the same solution as this project, there
	/// shouldn't be significant issues in having this as a variable here instead of
	/// handling the #ifs in each command project separately.
	/// </remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public const string ASSEMBLY_CONFIGURATION
#if DEBUG
		= "Debug";
#else
		= "Release";
#endif
	/// <summary>
	/// The current year.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public const string ASSEMBLY_COPYRIGHT = "Copyright © 2025";
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
	/// Debugger display string/
	/// </summary>
	public const string DEBUGGER_DISPLAY = "{DebuggerDisplay,nq}";
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
	/// <remarks>
	/// Switched from /xd to this invite since no matter what this invite will
	/// link to my server and never someone else's server.
	/// </remarks>
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
	public static string BOT_VERSION { get; } =
		typeof(Constants).Assembly.GetName().Version?.ToString()
		?? throw new InvalidOperationException("Cannot get bot version.");
	/// <summary>
	/// The Discord API wrapper version.
	/// </summary>
	public static string DISCORD_NET_VERSION { get; } =
		typeof(IDiscordClient).Assembly
		.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
		?? throw new InvalidOperationException("Cannot get Discord.Net version.");
	/// <summary>
	/// The time the bot was started in UTC.
	/// </summary>
	public static DateTime START { get; } = Process.GetCurrentProcess().StartTime.ToUniversalTime();
}