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
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class RequireBotOwnerAttribute : SelfGroupPreconditionAttribute
	{
		/// <inheritdoc />
		public override bool Visible => true;

		/// <inheritdoc />
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
			=> "Bot owner";
	}
}