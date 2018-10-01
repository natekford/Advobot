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
		/// <summary>
		/// Makes sure the context can be cast to <see cref="AdvobotCommandContext"/> and the user is a <see cref="SocketGuildUser"/>,
		/// otherwise throws an exception which is clearer than an <see cref="InvalidCastException"/>.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		internal static (AdvobotCommandContext Context, SocketGuildUser Invoker) InternalCastContext(this ICommandContext context)
		{
			if (!(context is AdvobotCommandContext aContext))
			{
				throw new ArgumentException($"Invalid context provided, must be {nameof(AdvobotCommandContext)}.");
			}
			if (!(context.User is SocketGuildUser user))
			{
				throw new ArgumentException("Unable to get the invoking user as a guild user.");
			}
			return (aContext, user);
		}
	}
}