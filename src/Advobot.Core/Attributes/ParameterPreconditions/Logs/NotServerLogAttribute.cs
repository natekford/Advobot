using System;

using Advobot.Services.GuildSettings;

using Discord;

namespace Advobot.Attributes.ParameterPreconditions.Logs
{
	/// <summary>
	/// Makes sure the passed in <see cref="ITextChannel"/> is not the current server log channel.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class NotServerLogAttribute : LogParameterPreconditionAttribute
	{
		/// <inheritdoc />
		protected override string LogName => "server";

		/// <inheritdoc />
		protected override ulong GetId(IGuildSettings settings)
			=> settings.ServerLogId;
	}
}