﻿using Advobot.Services.HelpEntries;
using Advobot.Utilities;

using Discord.Commands;

namespace Advobot.Attributes.Preconditions;

/// <summary>
/// Specifies a command is broken. Will provide an error each time a user tries to invoke the command.
/// </summary>
[Obsolete("This command is disabled.")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class DisabledCommandAttribute
	: PreconditionAttribute, IPrecondition
{
	/// <inheritdoc />
	public string Summary
		=> "This command will never be invokable because it is disabled";

	/// <inheritdoc />
	public override Task<PreconditionResult> CheckPermissionsAsync(
		ICommandContext context,
		CommandInfo command,
		IServiceProvider services)
		=> PreconditionResult.FromError("This command is disabled globally.").AsTask();
}