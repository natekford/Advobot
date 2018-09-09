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
	public class InviteListService : IInviteListService, IUsesDatabase, IDisposable
	{
		/// <inheritdoc />
		public string DatabaseName => "InviteList";
		/// <summary>
		/// The database being used. This can be any database type, or even just a simple dictionary.
		/// </summary>
		protected IDatabaseWrapper DbWrapper { get; set; }
		/// <summary>
		/// The factory for creating <see cref="DbWrapper"/>.
		/// </summary>
		protected IDatabaseWrapperFactory DatabaseFactory { get; }
		/// <summary>
		/// The settings this bot uses.
		/// </summary>
		protected IBotSettings Settings { get; }

		/// <summary>
		/// Creates an instance of <see cref="InviteListService"/>.
		/// </summary>
		/// <param name="provider"></param>
		public InviteListService(IServiceProvider provider)
		{
			Settings = provider.GetRequiredService<IBotSettings>();
			DatabaseFactory = provider.GetRequiredService<IDatabaseWrapperFactory>();
		}

		/// <inheritdoc />
		public IListedInvite Add(SocketGuild guild, IInvite invite, IEnumerable<string> keywords)
		{
			var listedInvite = new ListedInvite(guild, invite, keywords);
			DbWrapper.ExecuteQuery(DBQuery<ListedInvite>.Delete(x => x.GuildId == guild.Id));
			DbWrapper.ExecuteQuery(DBQuery<ListedInvite>.Insert(new[] { listedInvite }));
			return listedInvite;
		}
		/// <inheritdoc />
		public void Remove(ulong guildId)
			=> DbWrapper.ExecuteQuery(DBQuery<ListedInvite>.Delete(x => x.GuildId == guildId));
		/// <inheritdoc />
		public async Task UpdateAsync(SocketGuild guild)
		{
			var invite = (ListedInvite)GetListedInvite(guild.Id);
			await invite.UpdateAsync(guild).CAF();
			DbWrapper.ExecuteQuery(DBQuery<ListedInvite>.Update(new[] { invite }));
		}
		/// <inheritdoc />
		public async Task BumpAsync(SocketGuild guild)
		{
			var invite = (ListedInvite)GetListedInvite(guild.Id);
			await invite.BumpAsync(guild).CAF();
			DbWrapper.ExecuteQuery(DBQuery<ListedInvite>.Update(new[] { invite }));
		}
		/// <inheritdoc />
		public IEnumerable<IListedInvite> GetAll(int limit)
			=> DbWrapper.ExecuteQuery(DBQuery<ListedInvite>.GetAll()).OrderByDescending(x => x.Time);
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
			=> DbWrapper.ExecuteQuery(DBQuery<ListedInvite>.Get(x => x.GuildId == guildId)).Single();
		/// <inheritdoc />
		public void Start()
		{
			DbWrapper = DatabaseFactory.CreateWrapper(DatabaseName);
			ConsoleUtils.DebugWrite($"Started the database connection for {DatabaseName}.");
		}
		/// <inheritdoc />
		public void Dispose() => DbWrapper.Dispose();
	}
}
