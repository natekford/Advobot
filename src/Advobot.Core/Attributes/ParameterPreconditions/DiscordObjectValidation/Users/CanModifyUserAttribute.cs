using System;
using System.Threading.Tasks;

using Advobot.Utilities;

using Discord;
using Discord.Commands;

namespace Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Users
{
	/// <summary>
	/// Validates the passed in <see cref="IGuildUser"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class CanModifyUserAttribute : UserParameterPreconditionAttribute
	{
		/// <inheritdoc />
		public override string Summary
			=> "Can be modified by both the bot and the invoking user";

		/// <inheritdoc />
		protected override Task<PreconditionResult> SingularCheckUserAsync(
			ICommandContext context,
			ParameterInfo parameter,
			IGuildUser invoker,
			IGuildUser user,
			IServiceProvider services)
			=> invoker.ValidateUser(user);
	}
}