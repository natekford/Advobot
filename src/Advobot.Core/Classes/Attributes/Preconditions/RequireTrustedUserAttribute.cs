using System;
using System.Threading.Tasks;
using Advobot.Interfaces;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Classes.Attributes.Preconditions
{
	/// <summary>
	/// Requires trusted user status before this command will execute.
	/// </summary>
	[Obsolete("Remove this for safety reasons? Or let trusted users exist?")]
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class RequireTrustedUserAttribute : PreconditionAttribute
	{
		/// <summary>
		/// Verifies that this command was invoked by a trusted user.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="command"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
		{
			return Task.FromResult(services.GetRequiredService<IBotSettings>().TrustedUsers.Contains(context.User.Id)
				? PreconditionResult.FromSuccess()
				: PreconditionResult.FromError(default(string)));
		}
		/// <summary>
		/// Returns a string describing what this attribute requires.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
			=> "Trusted User";
	}
}