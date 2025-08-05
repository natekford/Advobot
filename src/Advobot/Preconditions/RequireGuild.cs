using Advobot.Services.HelpEntries;
using Advobot.Utilities;

using Discord.Commands;

namespace Advobot.Preconditions;

/// <summary>
/// Specifies that the command will only work in the passed in guild.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class RequireGuild(ulong id) : PreconditionAttribute, IPrecondition
{
	/// <summary>
	/// The id of the guild.
	/// </summary>
	public ulong Id { get; } = id;
	/// <inheritdoc />
	public string Summary => $"Will only work in the guild with the id {Id}";

	/// <inheritdoc />
	public override Task<PreconditionResult> CheckPermissionsAsync(
		ICommandContext context,
		CommandInfo command,
		IServiceProvider services)
	{
		if (context.Guild.Id == Id)
		{
			return this.FromSuccess().AsTask();
		}
		return PreconditionResult.FromError($"This guild does not have the id {Id}.").AsTask();
	}
}