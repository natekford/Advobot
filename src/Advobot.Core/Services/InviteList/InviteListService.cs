using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Advobot.Databases;
using Advobot.Databases.Abstract;
using Advobot.Services.Time;
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
		private readonly ITime _Time;

		/// <inheritdoc />
		public override string DatabaseName => "InviteList";

		/// <summary>
		/// Creates an instance of <see cref="InviteListService"/>.
		/// </summary>
		/// <param name="time"></param>
		/// <param name="dbFactory"></param>
		public InviteListService(ITime time, IDatabaseWrapperFactory dbFactory)
			: base(dbFactory)
		{
			_Time = time;
		}

		/// <inheritdoc />
		public IListedInvite Add(
			SocketGuild guild,
			IInviteMetadata invite,
			IEnumerable<string> keywords)
		{
			var listedInvite = new ListedInvite(guild, invite, keywords);
			DatabaseWrapper.ExecuteQuery(DatabaseQuery<ListedInvite>.Delete(x => x.GuildId == guild.Id));
			DatabaseWrapper.ExecuteQuery(DatabaseQuery<ListedInvite>.Insert(new[] { listedInvite }));
			return listedInvite;
		}

		/// <inheritdoc />
		public async Task BumpAsync(SocketGuild guild)
		{
			var invite = (ListedInvite)Get(guild.Id);
			await invite.BumpAsync(_Time.UtcNow, guild).CAF();
			DatabaseWrapper.ExecuteQuery(DatabaseQuery<ListedInvite>.Update(new[] { invite }));
		}

		/// <inheritdoc />
		public IListedInvite Get(ulong guildId)
			=> DatabaseWrapper.ExecuteQuery(DatabaseQuery<ListedInvite>.Get(x => x.GuildId == guildId)).SingleOrDefault();

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
		public void Remove(ulong guildId)
			=> DatabaseWrapper.ExecuteQuery(DatabaseQuery<ListedInvite>.Delete(x => x.GuildId == guildId));

		/// <inheritdoc />
		public async Task UpdateAsync(SocketGuild guild)
		{
			var invite = (ListedInvite)Get(guild.Id);
			await invite.UpdateAsync(guild).CAF();
			DatabaseWrapper.ExecuteQuery(DatabaseQuery<ListedInvite>.Update(new[] { invite }));
		}
	}
}