using Advobot.Utilities;

using Discord;
using Discord.Commands;

namespace Advobot.ParameterPreconditions.Strings;

/// <summary>
/// Validates the text channel name by making sure it is between 2 and 100 characters and has no spaces.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class TextChannelName : ChannelName
{
	/// <inheritdoc />
	public override string StringType => "text channel name with no spaces";

	/// <inheritdoc />
	protected override async Task<PreconditionResult> CheckPermissionsAsync(
		ICommandContext context,
		ParameterInfo parameter,
		IGuildUser invoker,
		string value,
		IServiceProvider services)
	{
		var result = await base.CheckPermissionsAsync(context, parameter, invoker, value, services).ConfigureAwait(false);
		if (!result.IsSuccess)
		{
			return result;
		}

		if (!value.Contains(" "))
		{
			return this.FromSuccess();
		}
		return PreconditionResult.FromError("Spaces are not allowed in text channel names.");
	}
}