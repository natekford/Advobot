﻿using Advobot.GeneratedParameterPreconditions;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

namespace Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation;

/// <summary>
/// Makes sure the passed in <see cref="ulong"/> is not already banned.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class NotBannedAttribute : UInt64ParameterPreconditionAttribute, IExistenceParameterPrecondition
{
	/// <inheritdoc />
	public ExistenceStatus Status => ExistenceStatus.MustNotExist;
	/// <inheritdoc />
	public override string Summary => "Not already banned";

	/// <inheritdoc />
	protected override async Task<PreconditionResult> CheckPermissionsAsync(
		ICommandContext context,
		ParameterInfo parameter,
		IGuildUser invoker,
		ulong value,
		IServiceProvider services)
	{
		var bans = await context.Guild.GetBansAsync().CAF();
		var exists = bans.Any(x => x.User.Id == value);
		return this.FromExistence(exists, value, "ban");
	}
}