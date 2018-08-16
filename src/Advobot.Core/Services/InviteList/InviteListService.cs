using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.WebSocket;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Services.InviteList
{
	/// <summary>
	/// Handles holding all <see cref="IListedInvite"/>.
	/// </summary>
	internal sealed class InviteListService : IInviteListService, IUsesDatabase, IDisposable
	{
		private LiteDatabase _Db;
		private readonly IBotSettings _Settings;

		/// <summary>
		/// Creates an instance of <see cref="InviteListService"/>.
		/// </summary>
		/// <param name="provider"></param>
		public InviteListService(IIterableServiceProvider provider)
		{
			_Settings = provider.GetRequiredService<IBotSettings>();
		}

		/// <inheritdoc />
		public void Start()
		{
			_Db = _Settings.GetDatabase("InviteDatabase.db");
			ConsoleUtils.DebugWrite($"Started the database connection for {nameof(InviteListService)}.");
		}
		/// <inheritdoc />
		public void Dispose()
		{
			_Db?.Dispose();
		}
		/// <inheritdoc />
		public IListedInvite Add(SocketGuild guild, IInvite invite, IEnumerable<string> keywords)
		{
			var col = _Db.GetCollection<ListedInvite>();
			col.Delete(x => x.GuildId == guild.Id);
			var listedInvite = new ListedInvite(guild, invite, keywords);
			col.Insert(listedInvite);
			return listedInvite;
		}
		/// <inheritdoc />
		public void Remove(ulong guildId)
		{
			_Db.GetCollection<ListedInvite>().Delete(x => x.GuildId == guildId);
		}
		/// <inheritdoc />
		public async Task UpdateAsync(SocketGuild guild)
		{
			if (!(GetListedInvite(guild.Id) is ListedInvite invite))
			{
				return;
			}
			await invite.UpdateAsync(guild).CAF();
			_Db.GetCollection<ListedInvite>().Update(invite);
		}
		/// <inheritdoc />
		public async Task BumpAsync(SocketGuild guild)
		{
			if (!(GetListedInvite(guild.Id) is ListedInvite invite))
			{
				return;
			}
			await invite.BumpAsync(guild).CAF();
			_Db.GetCollection<ListedInvite>().Update(invite);
		}
		/// <inheritdoc />
		public IEnumerable<IListedInvite> GetAll(int limit)
		{
			return _Db.GetCollection<ListedInvite>().Find(Query.All(nameof(ListedInvite.Time), Query.Descending), 0, limit);
		}
		/// <inheritdoc />
		public IEnumerable<IListedInvite> GetAll(int limit, params string[] keywords)
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
		{
			return _Db.GetCollection<ListedInvite>().FindOne(x => x.GuildId == guildId);
		}
	}
}
