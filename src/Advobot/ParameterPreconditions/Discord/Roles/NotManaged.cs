using Advobot.Modules;

using Discord;

using YACCS.Preconditions;
using YACCS.Results;

namespace Advobot.ParameterPreconditions.Discord.Roles;

/// <summary>
/// Does not allow managed roles but does allow the everyone role.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class NotManaged : AdvobotParameterPrecondition<IRole>
{
	/// <inheritdoc />
	public override string Summary => "Not managed";

	/// <inheritdoc />
	public override ValueTask<IResult> CheckAsync(
		CommandMeta meta,
		IGuildContext context,
		IRole? value)
	{
		if (value is { IsManaged: true })
		{
			// TODO: singleton
			var error = "The role cannot be managed.";
			return new(Result.Failure(error));
		}
		return new(CachedResults.Success);
	}
}