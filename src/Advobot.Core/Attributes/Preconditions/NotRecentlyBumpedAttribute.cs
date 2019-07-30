using Advobot.Services.InviteList;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Advobot.Attributes.Preconditions
{
	/// <summary>
	/// Makes sure the guild <see cref="IListedInvite"/> has not been bumped within the past hour.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class NotRecentlyBumpedAttribute : PreconditionAttribute
	{
		/// <inheritdoc />
		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
		{
			var inviteService = services.GetRequiredService<IInviteListService>();
			var invite = inviteService.Get(context.Guild.Id);
			if (invite == null)
			{
				return Task.FromResult(PreconditionResult.FromError("There is no listed invite."));
			}
			else if ((DateTime.UtcNow - invite.Time).TotalHours > 1)
			{
				return Task.FromResult(PreconditionResult.FromSuccess());
			}
			return Task.FromResult(PreconditionResult.FromError("The last invite bump was too recent."));
		}
	}
}
