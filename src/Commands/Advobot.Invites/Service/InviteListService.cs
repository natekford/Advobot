using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.Invites.Database;
using Advobot.Invites.Models;
using Advobot.Invites.ReadOnlyModels;
using Advobot.Services.Time;

using AdvorangesUtils;

using Discord;

namespace Advobot.Invites.Service
{
	public sealed class InviteListService : IInviteListService
	{
		private readonly InviteDatabase _Db;
		private readonly ITime _Time;

		public InviteListService(
			InviteDatabase db,
			ITime time)
		{
			_Db = db;
			_Time = time;
		}

		public Task AddAsync(IInviteMetadata invite)
		{
			var listedInvite = new ListedInvite(invite, _Time.UtcNow);
			return _Db.AddInviteAsync(listedInvite);
		}

		public async Task<bool> BumpAsync(IGuild guild)
		{
			var listedInvite = await _Db.GetInviteAsync(guild.Id).CAF();
			var invites = await guild.GetInvitesAsync().CAF();
			if (invites.TryGetFirst(x => x.Id == listedInvite.Code, out var invite))
			{
				var newListedInvite = new ListedInvite(invite, _Time.UtcNow);
				await _Db.UpdateInviteAsync(listedInvite).CAF();
				return true;
			}

			await RemoveAsync(guild.Id).CAF();
			return false;
		}

		public Task<IEnumerable<IReadOnlyListedInvite>> GetAllAsync()
			=> _Db.GetInvitesAsync();

		public Task<IEnumerable<IReadOnlyListedInvite>> GetAllAsync(
			IEnumerable<string> keywords)
			=> _Db.GetInvitesAsync(keywords);

		public Task<IReadOnlyListedInvite?> GetAsync(ulong guildId)
			=> _Db.GetInviteAsync(guildId);

		public Task RemoveAsync(ulong guildId)
			=> _Db.DeleteInviteAsync(guildId);
	}
}