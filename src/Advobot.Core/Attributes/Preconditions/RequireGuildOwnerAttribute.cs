using System;
using System.Threading.Tasks;
using Advobot.Modules;
using Discord.Commands;

namespace Advobot.Attributes.Preconditions
{
	/// <summary>
	/// Requires guild owner before this command will execute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class RequireGuildOwner : AdvobotPreconditionAttribute
	{
		/// <inheritdoc />
		public override bool Visible => true;

		/// <inheritdoc />
		public override Task<PreconditionResult> CheckPermissionsAsync(AdvobotCommandContext context, CommandInfo command, IServiceProvider services)
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
			=> "Guild owner";
	}
}