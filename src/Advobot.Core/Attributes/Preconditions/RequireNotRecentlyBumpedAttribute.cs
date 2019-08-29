using System;
using System.Threading.Tasks;
using Advobot.Services.HelpEntries;
using Advobot.Services.InviteList;
using Advobot.Utilities;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Attributes.Preconditions
{
	/// <summary>
	/// Makes sure the guild <see cref="IListedInvite"/> has not been bumped within the past hour.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class RequireNotRecentlyBumpedAttribute
		: PreconditionAttribute, IPrecondition
	{
		/// <inheritdoc />
		public string Summary
			=> "Listed invite has not been bumped within the past hour";

		/// <inheritdoc />
		public override Task<PreconditionResult> CheckPermissionsAsync(
			ICommandContext context,
			CommandInfo command,
			IServiceProvider services)
		{
			var inviteService = services.GetRequiredService<IInviteListService>();
			var invite = inviteService.Get(context.Guild.Id);
			if (invite == null)
			{
				return PreconditionUtils.FromErrorAsync("There is no listed invite.");
			}
			else if ((DateTime.UtcNow - invite.Time).TotalHours > 1)
			{
				return PreconditionUtils.FromSuccessAsync();
			}
			return PreconditionUtils.FromErrorAsync("The last invite bump was too recent.");
		}
	}
}
