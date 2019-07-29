using System;
using System.Threading.Tasks;
using Advobot.Modules;
using Discord.Commands;

namespace Advobot.Attributes.Preconditions
{
	/// <summary>
	/// Specifies a command is broken. Will provide an error each time a user tries to invoke the command.
	/// </summary>
	[Obsolete("This command is disabled.")]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class DisabledCommandAttribute : AdvobotPreconditionAttribute
	{
		/// <inheritdoc />
		public override bool Visible => false;

		/// <inheritdoc />
		public override Task<PreconditionResult> CheckPermissionsAsync(AdvobotCommandContext context, CommandInfo command, IServiceProvider services)
			=> Task.FromResult(PreconditionResult.FromError("This command is disabled globally."));
		/// <summary>
		/// Returns a string describing what this attributes requires.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
			=> "This command will never be invokable.";
	}
}
