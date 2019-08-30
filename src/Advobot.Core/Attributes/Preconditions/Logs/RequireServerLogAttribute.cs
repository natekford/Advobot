using System;

using Advobot.Services.GuildSettings;

namespace Advobot.Attributes.Preconditions.Logs
{
	/// <summary>
	/// Requires a server log to be set.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class RequireServerLogAttribute : RequireLogAttribute
	{
		/// <inheritdoc />
		protected override string LogName => "server";

		/// <inheritdoc />
		protected override ulong GetId(IGuildSettings settings)
			=> settings.ServerLogId;
	}
}