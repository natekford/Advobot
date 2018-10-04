using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace Advobot.Classes.Attributes.Preconditions
{
	/// <summary>
	/// Specifies a command is broken. Will provide an error each time a user tries to invoke the command.
	/// </summary>
	[Obsolete("This command is disabled.")]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class DisabledCommandAttribute : SelfGroupPreconditionAttribute
	{
		/// <inheritdoc />
		public override bool Visible => false;

		/// <inheritdoc />
		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
			=> Task.FromResult(PreconditionResult.FromError("This command is currently disabled."));
	}
}
