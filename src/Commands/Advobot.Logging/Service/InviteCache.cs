using Discord;
using Discord.Net;

using System.Collections.Concurrent;
using System.Net;

using static Advobot.Resources.Responses;

namespace Advobot.Logging.Service;

/// <summary>
/// Caches invites and attempts to figure out which invite a user joined from.
/// </summary>
public sealed class InviteCache
{
	private readonly ConcurrentDictionary<string, int> _Cached = [];

	/// <summary>
	/// Caches the current invites on this guild.
	/// </summary>
	/// <param name="guild"></param>
	/// <returns></returns>
	public async Task CacheInvitesAsync(IGuild guild)
		=> CacheInvites(await SafeGetInvitesAsync(guild).ConfigureAwait(false));

	/// <summary>
	/// Attempts to find the invite a user has joined on.
	/// </summary>
	/// <param name="guild"></param>
	/// <param name="user"></param>
	/// <returns></returns>
	public async Task<string?> GetInviteUserJoinedOnAsync(IGuild guild, IUser user)
	{
		//Bots join by being invited by admin, not through invites.
		if (user.IsBot)
		{
			return VariableBotInvitedByAdmin;
		}

		//If the bot can't get invites then determining the correct invite is not possible with any accuracy
		//No invites means single use, vanity url, or linked twitch
		var current = await SafeGetInvitesAsync(guild).ConfigureAwait(false);
		if (current is null)
		{
			return null;
		}
		if (current.Count == 0)
		{
			return VariableSingleUseInviteVanityUrlOrTwitch;
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
		if ((uncached.Length == 0 || uncached.All(x => x.Uses == 0))
			&& !string.IsNullOrWhiteSpace(guild.VanityURLCode))
		{
			return VariableSingleUseInviteVanityUrlOrTwitch;
		}
		var firstUses = uncached.Where(x => x.Uses != 0).ToArray();
		if (firstUses.Length == 1)
		{
			return UpdateCachedInvite(firstUses[0]);
		}
		return null;
	}

	private void CacheInvites(IEnumerable<IInviteMetadata>? invites)
	{
		if (invites is null)
		{
			return;
		}

		foreach (var invite in invites)
		{
			_Cached[invite.Id] = invite.Uses ?? 0;
		}
	}

	private async Task<IReadOnlyCollection<IInviteMetadata>?> SafeGetInvitesAsync(IGuild guild)
	{
		var currentUser = await guild.GetCurrentUserAsync().ConfigureAwait(false);
		if (currentUser.GuildPermissions.ManageGuild)
		{
			try
			{
				return await guild.GetInvitesAsync(new RequestOptions { Timeout = 250, }).ConfigureAwait(false);
			}
			catch (HttpException e) when (e.HttpCode == HttpStatusCode.InternalServerError)
			{
				return null;
			}
		}
		return [];
	}

	private string UpdateCachedInvite(IInviteMetadata invite)
	{
		_Cached[invite.Id] = invite.Uses ?? 0;
		return invite.Id;
	}
}