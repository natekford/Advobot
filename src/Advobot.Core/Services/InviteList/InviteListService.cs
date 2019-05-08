using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes.DatabaseWrappers;
using Advobot.Interfaces;
using AdvorangesUtils;
using Discord;
using Discord.WebSocket;

namespace Advobot.Services.InviteList
{
	/// <summary>
	/// Handles holding all <see cref="IListedInvite"/>.
	/// </summary>
	internal sealed class InviteListService : DatabaseWrapperConsumer, IInviteListService
	{
		/// <inheritdoc />
		public override string DatabaseName => "InviteList";

		/// <summary>
		/// Creates an instance of <see cref="InviteListService"/>.
		/// </summary>
		/// <param name="provider"></param>
		public InviteListService(IServiceProvider provider) : base(provider) { }

		/// <inheritdoc />
		public IListedInvite Add(SocketGuild guild, IInviteMetadata invite, IEnumerable<string> keywords)
		{
			var listedInvite = new ListedInvite(guild, invite, keywords);
			DatabaseWrapper.ExecuteQuery(DatabaseQuery<ListedInvite>.Delete(x => x.GuildId == guild.Id));
			DatabaseWrapper.ExecuteQuery(DatabaseQuery<ListedInvite>.Insert(new[] { listedInvite }));
			return listedInvite;
		}
		/// <inheritdoc />
		public void Remove(ulong guildId)
			=> DatabaseWrapper.ExecuteQuery(DatabaseQuery<ListedInvite>.Delete(x => x.GuildId == guildId));
		/// <inheritdoc />
		public async Task UpdateAsync(SocketGuild guild)
		{
			var invite = (ListedInvite)Get(guild.Id);
			await invite.UpdateAsync(guild).CAF();
			DatabaseWrapper.ExecuteQuery(DatabaseQuery<ListedInvite>.Update(new[] { invite }));
		}
		/// <inheritdoc />
		public async Task BumpAsync(SocketGuild guild)
		{
			var invite = (ListedInvite)Get(guild.Id);
			await invite.BumpAsync(guild).CAF();
			DatabaseWrapper.ExecuteQuery(DatabaseQuery<ListedInvite>.Update(new[] { invite }));
		}
		/// <inheritdoc />
		public IEnumerable<IListedInvite> GetAll(int limit)
			=> DatabaseWrapper.ExecuteQuery(DatabaseQuery<ListedInvite>.GetAll()).OrderByDescending(x => x.Time);
		/// <inheritdoc />
		public IEnumerable<IListedInvite> GetAll(int limit, IEnumerable<string> keywords)
		{
			var count = 0;
			foreach (var invite in GetAll(int.MaxValue))
			{
				if (!invite.Keywords.Intersect(keywords, StringComparer.OrdinalIgnoreCase).Any())
				{
					continue;
				}
				yield return invite;
				if (++count >= limit)
				{
					yield break;
				}
			}
		}
		/// <inheritdoc />
		public IListedInvite Get(ulong guildId)
			=> DatabaseWrapper.ExecuteQuery(DatabaseQuery<ListedInvite>.Get(x => x.GuildId == guildId)).Single();
	}
}
