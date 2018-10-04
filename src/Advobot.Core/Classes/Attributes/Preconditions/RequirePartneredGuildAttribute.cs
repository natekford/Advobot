using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace Advobot.Classes.Attributes.Preconditions
{
	/// <summary>
	/// Requires the guild in the command context to be 
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class RequirePartneredGuildAttribute : SelfGroupPreconditionAttribute
	{
		/// <inheritdoc />
		public override bool Visible => true;

		/// <inheritdoc />
		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
		{
			return context.Guild.Features.Count > 0
				? Task.FromResult(PreconditionResult.FromSuccess())
				: Task.FromResult(PreconditionResult.FromError("This guild is not partnered."));
		}
		/// <summary>
		/// Returns a string describing what this attribute requires.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
			=> "Partnered guild";
	}
}