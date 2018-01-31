using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace Advobot.Core.Classes.Attributes
{
	/// <summary>
	/// Specifies a command is broken. Will provide an error each time a user tries to invoke the command.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class DisabledCommandAttribute : PreconditionAttribute
	{
		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
		{
			return Task.FromResult(PreconditionResult.FromError("This command is currently disabled."));
		}
	}
}
