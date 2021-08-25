
using Advobot.Logging.Models;

namespace Advobot.Logging.Preconditions
{
	/// <summary>
	/// Requires a server log to be set.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class RequireServerLogAttribute : LogPreconditionAttribute
	{
		/// <inheritdoc />
		protected override string LogName => Resources.Responses.VariableServerLog;

		/// <inheritdoc />
		protected override ulong GetId(LogChannels channels)
			=> channels.ServerLogId;
	}
}