using Advobot.Modules;

using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.Results;

namespace Advobot.Preconditions;

/// <summary>
/// Specifies that the command will only work in the passed in guild.
/// </summary>
[AttributeUsage(AttributeUtils.COMMANDS, AllowMultiple = false, Inherited = true)]
public sealed class RequireGuild(ulong id) : AdvobotPrecondition
{
	/// <summary>
	/// The id of the guild.
	/// </summary>
	public ulong Id { get; } = id;
	/// <inheritdoc />
	public override string Summary => $"Will only work in the guild with the id {Id}";

	/// <inheritdoc />
	public override ValueTask<IResult> CheckAsync(
		IImmutableCommand command,
		IGuildContext context)
	{
		if (context.Guild.Id == Id)
		{
			return new(Result.EmptySuccess);
		}
		return new(Result.Failure($"This guild does not have the id `{Id}`."));
	}
}