using System;
using Advobot.Classes;
using Advobot.Interfaces;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Utilities
{
	/// <summary>
	/// Utilities intended to be only used internally.
	/// </summary>
	internal static class InternalUtils
	{
		/// <summary>
		/// Checks whether to use the bot prefix, or the guild settings prefix.
		/// </summary>
		/// <param name="b"></param>
		/// <param name="g"></param>
		/// <returns></returns>
		internal static string InternalGetPrefix(this IBotSettings b, IGuildSettings g)
			=> string.IsNullOrWhiteSpace(g?.Prefix) ? b.Prefix : g?.Prefix;
	}
}