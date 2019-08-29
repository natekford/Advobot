using System;
using System.Threading.Tasks;
using Advobot.Services.HelpEntries;
using Advobot.Utilities;
using Discord.Commands;

namespace Advobot.Attributes.Preconditions
{
	/// <summary>
	/// Requires the guild in the command context to be partnered.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class RequirePartneredGuildAttribute
		: PreconditionAttribute, IPrecondition
	{
		/// <inheritdoc />
		public string Summary
			=> "Guild is partnered";

		/// <inheritdoc />
		public override Task<PreconditionResult> CheckPermissionsAsync(
			ICommandContext context,
			CommandInfo command,
			IServiceProvider services)
		{
			if (context.Guild.Features.Count > 0)
			{
				return PreconditionUtils.FromSuccessAsync();
			}
			return PreconditionUtils.FromErrorAsync("This guild is not partnered.");
		}
	}
}