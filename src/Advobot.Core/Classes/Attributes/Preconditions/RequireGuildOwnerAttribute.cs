using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace Advobot.Classes.Attributes.Preconditions
{
	/// <summary>
	/// Requires guild owner before this command will execute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class RequireGuildOwner : PreconditionAttribute
	{
		/// <summary>
		/// Verifies this command was invoked by the guild owner.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="command"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
		{
			return Task.FromResult(context.Guild.OwnerId == context.User.Id
				? PreconditionResult.FromSuccess()
				: PreconditionResult.FromError(default(string)));
		}
		/// <summary>
		/// Returns a string describing what this attribute requires.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
			=> "Guild Owner";
	}
}