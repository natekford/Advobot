using System;
using Advobot.Services.GuildSettings;

namespace Advobot.Attributes.Preconditions.Logs
{
	/// <summary>
	/// Requires an image log to be set.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class RequireImageLogAttribute : RequireLogAttribute
	{
		/// <inheritdoc />
		protected override string LogName => "image";

		/// <inheritdoc />
		protected override ulong GetId(IGuildSettings settings)
			=> settings.ImageLogId;
	}
}
