using System;

using Advobot.Logging.ReadOnlyModels;

using Discord;

namespace Advobot.Logging.ParameterPreconditions
{
	/// <summary>
	/// Makes sure the passed in <see cref="ITextChannel"/> is not the current image log channel.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class NotImageLogAttribute : LogParameterPreconditionAttribute
	{
		/// <inheritdoc />
		protected override string LogName => Resources.Responses.VariableImageLog;

		/// <inheritdoc />
		protected override ulong GetId(IReadOnlyLogChannels channels)
			=> channels.ImageLogId;
	}
}