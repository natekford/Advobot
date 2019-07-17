using AdvorangesUtils;
using Discord;
using Discord.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Advobot.Classes
{
	/// <summary>
	/// Caches invites and attempts to figure out which invite a user joined from.
	/// </summary>
	public sealed class InviteCache
	{
		private readonly IDictionary<string, int> _Cached = new Dictionary<string, int>();

		/// <summary>
		/// Caches the current invites on this guild.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		public async Task CacheInvitesAsync(IGuild guild)
			=> CacheInvites(await SafeGetInvitesAsync(guild).CAF());
		/// <summary>
		/// Attempts to find the invite a user has joined on.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public async Task<string?> GetInviteUserJoinedOnAsync(IGuildUser user)
		{
			//Bots join by being invited by admin, not through invites.
			if (user.IsBot)
			{
				return "Bot invited by admin.";
			}

			//If the bot can't get invites then determining the correct invite is not possible with any accuracy
			//No invites means single use, vanity url, or linked twitch
			var current = await SafeGetInvitesAsync(user.Guild).CAF();
			if (current == null)
			{
				return null;
			}
			if (!current.Any())
			{
				return "Single use invite, vanity url, or linked Twitch account.";
			}

			//Find invites where the cached invite uses are not the same as the current ones.
			//If only one then treat it as the joining invite
			var updated = current.Where(x => _Cached.TryGetValue(x.Id, out var uses) && uses != x.Uses).ToArray();
			if (updated.Length == 1)
			{
				return UpdateCachedInvite(updated[0]);
			}

			//Get the new invites on the guild by finding which guild invites aren't on the bot invites list
			//If no new invites then assume it was the vanity url or linked twitch
			//If one then assume it's the new one, if more than one, no way to tell
			var uncached = current.Where(x => !_Cached.TryGetValue(x.Id, out _)).ToArray();
			CacheInvites(uncached);
			if ((!uncached.Any() || uncached.All(x => x.Uses == 0))
				&& !string.IsNullOrWhiteSpace(user.Guild.VanityURLCode))
			{
				return "Single use invite, vanity url, or linked Twitch account.";
			}
			var firstUses = uncached.Where(x => x.Uses != 0).ToArray();
			if (firstUses.Length == 1)
			{
				return UpdateCachedInvite(firstUses[0]);
			}
			return null;
		}

		private void CacheInvites(IEnumerable<IInviteMetadata> invites)
		{
			foreach (var invite in invites)
			{
				_Cached[invite.Id] = invite.Uses ?? 0;
			}
		}
		private string UpdateCachedInvite(IInviteMetadata invite)
		{
			_Cached[invite.Id] = invite.Uses ?? 0;
			return invite.Id;
		}
		private async Task<IReadOnlyCollection<IInviteMetadata>?> SafeGetInvitesAsync(IGuild guild)
		{
			var currentUser = await guild.GetCurrentUserAsync().CAF();
			if (currentUser.GuildPermissions.ManageGuild)
			{
				try
				{
					return await guild.GetInvitesAsync(new RequestOptions { Timeout = 250, }).CAF();
				}
				catch (HttpException e) when (e.HttpCode == HttpStatusCode.InternalServerError)
				{
					return null;
				}
			}
			return Array.Empty<IInviteMetadata>();
		}
	}
}
