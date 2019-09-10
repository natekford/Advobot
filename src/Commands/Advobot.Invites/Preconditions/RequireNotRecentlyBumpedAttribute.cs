using System;
using System.Threading.Tasks;

using Advobot.Invites.Service;
using Advobot.Invites.Utilities;
using Advobot.Services.HelpEntries;
using Advobot.Services.Time;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Invites.Preconditions
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
		public override async Task<PreconditionResult> CheckPermissionsAsync(
			ICommandContext context,
			CommandInfo command,
			IServiceProvider services)
		{
			var inviteService = services.GetRequiredService<IInviteListService>();
			var invite = await inviteService.GetAsync(context.Guild.Id).CAF();
			if (invite == null)
			{
				return PreconditionUtils.FromError("There is no listed invite.");
			}

			var time = services.GetRequiredService<ITime>();
			if ((time.UtcNow - invite.GetLastBumped()).TotalHours > 1)
			{
				return PreconditionUtils.FromSuccess();
			}
			return PreconditionUtils.FromError("The last invite bump was too recent.");
		}
	}
}