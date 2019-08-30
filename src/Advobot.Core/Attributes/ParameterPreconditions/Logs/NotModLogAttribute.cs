using System;

using Advobot.Services.GuildSettings;

using Discord;

namespace Advobot.Attributes.ParameterPreconditions.Logs
{
	/// <summary>
	/// Makes sure the passed in <see cref="ITextChannel"/> is not the current mod log channel.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class NotModLogAttribute : LogParameterPreconditionAttribute
	{
		/// <inheritdoc />
		protected override string LogName => "mod";

		/// <inheritdoc />
		protected override ulong GetId(IGuildSettings settings)
			=> settings.ModLogId;
	}
}