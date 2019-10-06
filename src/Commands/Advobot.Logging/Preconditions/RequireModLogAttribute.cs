using System;

using Advobot.Logging.ReadOnlyModels;

namespace Advobot.Logging.Preconditions
{
	/// <summary>
	/// Requires a mod log to be set.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class RequireModLogAttribute : LogPreconditionAttribute
	{
		/// <inheritdoc />
		protected override string LogName => Resources.Responses.VariableModLog;

		/// <inheritdoc />
		protected override ulong GetId(IReadOnlyLogChannels channels)
			=> channels.ModLogId;
	}
}