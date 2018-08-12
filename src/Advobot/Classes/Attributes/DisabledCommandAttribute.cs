using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace Advobot.Classes.Attributes
{
	/// <summary>
	/// Specifies a command is broken. Will provide an error each time a user tries to invoke the command.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class DisabledCommandAttribute : PreconditionAttribute
	{
		/// <summary>
		/// Returns an error stating that the command is disabled.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="command"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
		{
			return Task.FromResult(PreconditionResult.FromError("This command is currently disabled."));
		}
	}
}
