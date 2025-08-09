using Advobot.Utilities;

using Discord;
using Discord.Commands;

using System.Text.RegularExpressions;

namespace Advobot.ParameterPreconditions.Strings;

/// <summary>
/// Validates the Twitch stream name by making sure it is between 4 and 25 characters and matches a Regex for Twitch usernames.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed partial class TwitchStream : StringLengthParameterPrecondition
{
	/// <inheritdoc />
	public override string StringType => "Twitch stream name";

	/// <summary>
	/// Creates an instance of <see cref="TwitchStream"/>.
	/// </summary>
	public TwitchStream() : base(4, 25) { }

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

		if (GetTwitchRegex().IsMatch(value))
		{
			return this.FromSuccess();
		}
		return PreconditionResult.FromError("Invalid Twitch username supplied.");
	}

	[GeneratedRegex("^[a-zA-Z0-9_]{4,25}$", RegexOptions.Compiled)]
	private static partial System.Text.RegularExpressions.Regex GetTwitchRegex();
}