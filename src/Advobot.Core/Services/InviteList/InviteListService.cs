using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes.DatabaseWrappers;
using Advobot.Interfaces;
using AdvorangesUtils;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

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
		/// The settings this bot uses.
		/// </summary>
		private IBotSettings Settings { get; }

		/// <summary>
		/// Creates an instance of <see cref="InviteListService"/>.
		/// </summary>
		/// <param name="provider"></param>
		public InviteListService(IServiceProvider provider) : base(provider)
		{
			Settings = provider.GetRequiredService<IBotSettings>();
		}

		/// <inheritdoc />
		public IListedInvite Add(SocketGuild guild, IInvite invite, IEnumerable<string> keywords)
		{
			var listedInvite = new ListedInvite(guild, invite, keywords);
			DatabaseWrapper.ExecuteQuery(DBQuery<ListedInvite>.Delete(x => x.GuildId == guild.Id));
			DatabaseWrapper.ExecuteQuery(DBQuery<ListedInvite>.Insert(new[] { listedInvite }));
			return listedInvite;
		}
		/// <inheritdoc />
		public void Remove(ulong guildId)
			=> DatabaseWrapper.ExecuteQuery(DBQuery<ListedInvite>.Delete(x => x.GuildId == guildId));
		/// <inheritdoc />
		public async Task UpdateAsync(SocketGuild guild)
		{
			var invite = (ListedInvite)GetListedInvite(guild.Id);
			await invite.UpdateAsync(guild).CAF();
			DatabaseWrapper.ExecuteQuery(DBQuery<ListedInvite>.Update(new[] { invite }));
		}
		/// <inheritdoc />
		public async Task BumpAsync(SocketGuild guild)
		{
			var invite = (ListedInvite)GetListedInvite(guild.Id);
			await invite.BumpAsync(guild).CAF();
			DatabaseWrapper.ExecuteQuery(DBQuery<ListedInvite>.Update(new[] { invite }));
		}
		/// <inheritdoc />
		public IEnumerable<IListedInvite> GetAll(int limit)
			=> DatabaseWrapper.ExecuteQuery(DBQuery<ListedInvite>.GetAll()).OrderByDescending(x => x.Time);
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
		public IListedInvite GetListedInvite(ulong guildId)
			=> DatabaseWrapper.ExecuteQuery(DBQuery<ListedInvite>.Get(x => x.GuildId == guildId)).Single();
	}
}
