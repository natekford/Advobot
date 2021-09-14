
using Advobot.GeneratedParameterPreconditions;
using Advobot.Utilities;

using Discord;
using Discord.Commands;

namespace Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles
{
	/// <summary>
	/// Does not allow managed roles but does allow the everyone role.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class NotManagedAttribute : IRoleParameterPreconditionAttribute
	{
		/// <inheritdoc />
		public override string Summary => "Not managed";

		/// <inheritdoc />
		protected override Task<PreconditionResult> CheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			IGuildUser invoker,
			IRole role,
			IServiceProvider services)
		{
			if (!role.IsManaged)
			{
				return this.FromSuccess().AsTask();
			}
			return PreconditionResult.FromError("The role cannot be managed.").AsTask();
		}
	}
}