using Advobot.Classes;
using Advobot.Interfaces;
using Discord;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Actions
{
	public static class InviteActions
	{
		public static async Task<IReadOnlyCollection<IInviteMetadata>> GetInvites(IGuild guild)
		{
			var currUser = await guild.GetCurrentUserAsync();
			if (!currUser.GuildPermissions.ManageGuild)
			{
				return new List<IInviteMetadata>();
			}

			return await guild.GetInvitesAsync();
		}
		public static async Task<BotInvite> GetInviteUserJoinedOn(IGuildSettings guildSettings, IGuild guild)
		{
			BotInvite joinInv = null;

			var currentInvites = await GetInvites(guild);
			var cachedInvites = guildSettings.Invites;
			if (!currentInvites.Any())
			{
				return joinInv;
			}

			//Find invites where the cached invite uses are not the same as the current ones.
			var updatedInvites = cachedInvites.Where(cached =>
			{
				return currentInvites.Any(current => cached.Code == current.Code && cached.Uses != current.Uses);
			});

			//If only one then treat it as the joining invite
			if (updatedInvites.Count() == 1)
			{
				joinInv = updatedInvites.FirstOrDefault();
				joinInv.IncrementUses();
			}
			//If zero or more than one 
			else
			{
				//Get the new invites on the guild by finding which guild invites aren't on the bot invites list
				var newInvs = currentInvites.Where(current =>
				{
					return !cachedInvites.Select(cached => cached.Code).Contains(current.Code);
				});


				if (!newInvs.Any() || newInvs.All(x => x.Uses == 0))
				{
					if (guild.Features.CaseInsContains(Constants.VANITY_URL))
					{
						joinInv = new BotInvite("Vanity URL", 0);
					}
				}
				else if (newInvs.Count() == 1)
				{
					var newInv = newInvs.First();
					joinInv = new BotInvite(newInv.Code, newInv.Uses);
				}
				else
				{

				}
				guildSettings.Invites.AddRange(newInvs.Select(x => new BotInvite(x.Code, x.Uses)));
			}
			return joinInv;
		}

		public static async Task<IInviteMetadata> CreateInvite(IGuildChannel channel, int? maxAge, int? maxUses, bool isTemporary, bool isUnique, string reason)
		{
			return await channel.CreateInviteAsync(maxAge, maxUses, isTemporary, isUnique, new RequestOptions { AuditLogReason = reason });
		}
		public static async Task DeleteInvite(IInvite invite, string reason)
		{
			await invite.DeleteAsync(new RequestOptions { AuditLogReason = reason });
		}
	}
}