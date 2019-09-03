﻿using System;
using System.Threading.Tasks;

using Advobot.Utilities;

using Discord;
using Discord.Commands;

namespace Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Invites
{
	/// <summary>
	/// Validates the passed in <see cref="IInviteMetadata"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public abstract class InviteParameterPreconditionAttribute
		: AdvobotParameterPreconditionAttribute
	{
		/// <summary>
		/// Checks whether the condition for the <see cref="IInviteMetadata"/> is met before execution of the command.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="parameter"></param>
		/// <param name="invoker"></param>
		/// <param name="invite"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		protected abstract Task<PreconditionResult> SingularCheckInviteAsync(
			ICommandContext context,
			ParameterInfo parameter,
			IGuildUser invoker,
			IInviteMetadata invite,
			IServiceProvider services);

		/// <inheritdoc />
		protected override Task<PreconditionResult> SingularCheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			object value,
			IServiceProvider services)
		{
			if (!(context.User is IGuildUser user))
			{
				return PreconditionUtils.FromInvalidInvoker().AsTask();
			}
			if (value is IInviteMetadata invite)
			{
				return SingularCheckInviteAsync(context, parameter, user, invite, services);
			}
			else if (value is null)
			{
				var error = $"No value was passed in for {parameter.Name}.";
				return PreconditionUtils.FromError(error).AsTask();
			}
			return this.FromOnlySupports(typeof(IInviteMetadata)).AsTask();
		}
	}
}