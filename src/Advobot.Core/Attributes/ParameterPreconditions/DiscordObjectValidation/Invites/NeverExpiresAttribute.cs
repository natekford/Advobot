using System;
using System.Threading.Tasks;

using Advobot.Utilities;

using Discord;
using Discord.Commands;

namespace Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Invites
{
	/// <summary>
	/// Does not allow invites which can expire.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class NeverExpiresAttribute
		: InviteParameterPreconditionAttribute
	{
		/// <inheritdoc />
		public override string Summary
			=> "Never expires";

		/// <inheritdoc />
		protected override Task<PreconditionResult> SingularCheckInviteAsync(
			ICommandContext context,
			ParameterInfo parameter,
			IGuildUser invoker,
			IInviteMetadata invite,
			IServiceProvider services)
		{
			if (invite.MaxAge == null)
			{
				return PreconditionResult.FromSuccess().AsTask();
			}
			return PreconditionResult.FromError("The invite cannot expire.").AsTask();
		}
	}
}