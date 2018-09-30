using System;
using System.Threading.Tasks;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord.Commands;

namespace Advobot.Classes.Attributes.Preconditions
{
	/// <summary>
	/// Requires bot owner before this command will execute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class RequireBotOwnerAttribute : PreconditionAttribute
	{
		/// <summary>
		/// Verifies this command was invoked by the bot owner.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="command"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
		{
			return await ClientUtils.GetOwnerIdAsync(context.Client).CAF() == context.User.Id
				? PreconditionResult.FromSuccess()
				: PreconditionResult.FromError(default(string));
		}
		/// <summary>
		/// Returns a string describing what this attribute requires.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
			=> "Bot Owner";
	}
}