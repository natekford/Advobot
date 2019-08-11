using System;
using Advobot.Services.GuildSettings;

namespace Advobot.Attributes.Preconditions.Logs
{
	/// <summary>
	/// Requires a mod log to be set.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class RequireModLogAttribute : RequireLogAttribute
	{
		/// <inheritdoc />
		protected override string LogName => "mod";

		/// <inheritdoc />
		protected override ulong GetId(IGuildSettings settings)
			=> settings.ModLogId;
	}
}
