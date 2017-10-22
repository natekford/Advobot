using Advobot.Core.Classes;
using Advobot.Core.Interfaces;
using Discord;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Core.Actions
{
	public static class InviteActions
	{
		/// <summary>
		/// Checks if the bot can get invites before trying to get invites.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		public static async Task<IReadOnlyList<IInviteMetadata>> GetInvitesAsync(IGuild guild)
		{
			if (!guild.GetBot().GuildPermissions.ManageGuild)
			{
				return new List<IInviteMetadata>();
			}

			return (await guild.GetInvitesAsync().CAF()).ToList().AsReadOnly();
		}
		/// <summary>
		/// Tries to find the invite a user joined on.
		/// </summary>
		/// <param name="guildSettings"></param>
		/// <param name="user"></param>
		/// <returns></returns>
		public static async Task<CachedInvite> GetInviteUserJoinedOnAsync(IGuildSettings guildSettings, IGuildUser user)
		{
			CachedInvite joinInv = null;

			//Bots join by being invited by admin, not through invites.
			if (user.IsBot)
			{
				return new CachedInvite("Invited by admin", 0);
			}

			var currentInvites = await GetInvitesAsync(user.Guild).CAF();
			var cachedInvites = guildSettings.Invites;
			if (!currentInvites.Any())
			{
				return joinInv;
			}

			//Find invites where the cached invite uses are not the same as the current ones.
			var updatedInvites = cachedInvites.Where(cached => currentInvites.Any(current => cached.Code == current.Code && cached.Uses != current.Uses));
			//If only one then treat it as the joining invite
			if (updatedInvites.Count() == 1)
			{
				joinInv = updatedInvites.FirstOrDefault();
				joinInv.IncrementUses();
			}
			else
			{
				//Get the new invites on the guild by finding which guild invites aren't on the bot invites list
				var newInvs = currentInvites.Where(current => !cachedInvites.Select(cached => cached.Code).Contains(current.Code));
				//If no new invites then assume it was the vanity url
				if (!newInvs.Any() || newInvs.All(x => x.Uses == 0))
				{
					if (user.Guild.Features.CaseInsContains(Constants.VANITY_URL))
					{
						joinInv = new CachedInvite("Vanity URL", 0);
					}
				}
				//If one then assume it's the new one
				else if (newInvs.Count() == 1)
				{
					joinInv = new CachedInvite(newInvs.First().Code, newInvs.First().Uses);
				}
				//No way to tell if more than one
				guildSettings.Invites.AddRange(newInvs.Select(x => new CachedInvite(x.Code, x.Uses)));
			}
			return joinInv;
		}

		/// <summary>
		/// Creates an invite with the supplied arguments.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="maxAge"></param>
		/// <param name="maxUses"></param>
		/// <param name="isTemporary"></param>
		/// <param name="isUnique"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static async Task<IInviteMetadata> CreateInviteAsync(IGuildChannel channel, int? maxAge, int? maxUses, bool isTemporary,
			bool isUnique, ModerationReason reason)
		{
			return await channel.CreateInviteAsync(maxAge, maxUses, isTemporary, isUnique, reason.CreateRequestOptions()).CAF();
		}
		/// <summary>
		/// Deletes the invite.
		/// </summary>
		/// <param name="invite"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static async Task DeleteInviteAsync(IInvite invite, ModerationReason reason)
		{
			await invite.DeleteAsync(reason.CreateRequestOptions()).CAF();
		}
	}
}