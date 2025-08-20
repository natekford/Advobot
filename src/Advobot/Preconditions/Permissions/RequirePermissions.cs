using Advobot.Utilities;

using Discord;
using Discord.Commands;

using System.Collections.Immutable;

namespace Advobot.Preconditions.Permissions;

/// <summary>
/// For verifying a user's permissions.
/// </summary>
/// <param name="permissions"></param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public abstract class RequirePermissions(IEnumerable<Enum> permissions)
	: AdvobotPrecondition
{
	/// <summary>
	/// Whether this precondition has to validate the bot's permissions.
	/// </summary>
	public bool AppliesToBot { get; set; }
	/// <summary>
	/// Whether this precondition has to validate the invoker's permissions.
	/// </summary>
	public bool AppliesToInvoker { get; set; } = true;
	/// <summary>
	/// The flags required (each is a separate valid combination of flags).
	/// </summary>
	public ImmutableHashSet<Enum> Permissions { get; } = [.. permissions];
	/// <inheritdoc />
	public override string Summary { get; } = permissions.Select(x =>
	{
		var perms = default(List<string>);
		foreach (Enum e in Enum.GetValues(x.GetType()))
		{
			if (x.Equals(e))
			{
				return e.ToString();
			}
			else if (x.HasFlag(e))
			{
				perms ??= [];
				perms.Add(e.ToString());
			}
		}
		return perms?.Join(" & ") ?? "";
	}).Join(" | ");

	/// <inheritdoc />
	public override async Task<PreconditionResult> CheckPermissionsAsync(
		ICommandContext context,
		CommandInfo command,
		IServiceProvider services)
	{
		async Task<PreconditionResult> CheckPermissionsAsync(
			ICommandContext context,
			IGuildUser user,
			IServiceProvider services)
		{
			var perms = await GetUserPermissionsAsync(context, user, services).ConfigureAwait(false);
			if (perms is null)
			{
				return PreconditionResult.FromError($"`{user.Format()}` has no permissions.");
			}
			else if (!Permissions.Any(perms.HasFlag))
			{
				return PreconditionResult.FromError($"`{user.Format()}` does not have any suitable permissions.");
			}
			return this.FromSuccess();
		}

		if (AppliesToInvoker)
		{
			if (context.User is not IGuildUser user)
			{
				return this.FromInvalidInvoker();
			}

			var result = await CheckPermissionsAsync(context, user, services).ConfigureAwait(false);
			if (!result.IsSuccess)
			{
				return result;
			}
		}
		if (AppliesToBot)
		{
			var bot = await context.Guild.GetCurrentUserAsync().ConfigureAwait(false);

			var result = await CheckPermissionsAsync(context, bot, services).ConfigureAwait(false);
			if (!result.IsSuccess)
			{
				return result;
			}
		}
		return this.FromSuccess();
	}

	/// <summary>
	/// Returns the invoking user's permissions.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="user"></param>
	/// <param name="services"></param>
	/// <returns></returns>
	public abstract Task<Enum?> GetUserPermissionsAsync(
		ICommandContext context,
		IGuildUser user,
		IServiceProvider services);
}