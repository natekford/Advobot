using System;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.DatabaseWrappers;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Services.Levels
{
	/// <summary>
	/// Service for giving people experience for chatting and rewards for certain levels.
	/// </summary>
	internal sealed class LevelService : DatabaseWrapperConsumer, ILevelService
	{
		/// <inheritdoc />
		public override string DatabaseName => "LevelDatabase";
		/// <summary>
		/// The settings for each individual guild.
		/// </summary>
		private IGuildSettingsFactory GuildSettings { get; }
		/// <summary>
		/// The bot client.
		/// </summary>
		private DiscordShardedClient Client { get; }
		/// <summary>
		/// Used in calculating XP.
		/// </summary>
		private double Log { get; }
		/// <summary>
		/// Used in calculating XP.
		/// </summary>
		private double Pow { get; }
		/// <summary>
		/// The time between when XP can be gained again.
		/// </summary>
		private TimeSpan Time { get; }
		/// <summary>
		/// Base XP per message.
		/// </summary>
		private int BaseExperience { get; }

		/// <summary>
		/// Creates an instance of <see cref="LevelService"/>.
		/// </summary>
		/// <param name="provider"></param>
		public LevelService(IServiceProvider provider) : this(provider, new LevelServiceArguments()) { }
		/// <summary>
		/// Creates an instance of <see cref="LevelService"/>.
		/// </summary>
		/// <param name="provider"></param>
		/// <param name="args"></param>
		public LevelService(IServiceProvider provider, LevelServiceArguments args) : base(provider)
		{
			Client = provider.GetRequiredService<DiscordShardedClient>();
			GuildSettings = provider.GetRequiredService<IGuildSettingsFactory>();
			Log = args.Log;
			Pow = args.Pow;
			Time = args.Time;
			BaseExperience = args.BaseExperience;

			Client.MessageReceived += AddExperienceAsync;
			Client.MessageDeleted += RemoveExperienceAsync;
		}

		/// <inheritdoc />
		public async Task AddExperienceAsync(SocketMessage message)
		{
			if (message.Author.IsBot
				|| message.Author.IsWebhook
				|| !(message is SocketUserMessage msg)
				|| !(msg.Channel is SocketTextChannel channel)
				|| !(channel.Guild is SocketGuild guild)
				|| !(await GuildSettings.GetOrCreateAsync(guild).CAF() is IGuildSettings settings)
				|| settings.IgnoredXpChannels.Contains(message.Channel.Id)
				|| !(GetUserXpInformation(message.Author.Id) is UserExperienceInformation info)
				|| info.Time > (DateTime.UtcNow - Time))
			{
				return;
			}
			info.AddExperience(settings, msg, BaseExperience);
			DatabaseWrapper.ExecuteQuery(DatabaseQuery<UserExperienceInformation>.Update(new[] { info }));
			UpdateUserRank(info, ((SocketTextChannel)msg.Channel).Guild);
		}
		/// <inheritdoc />
		public Task RemoveExperienceAsync(Cacheable<IMessage, ulong> cached, ISocketMessageChannel channel)
		{
			if (!cached.HasValue
				|| cached.Value.Author.IsBot
				|| cached.Value.Author.IsWebhook
				|| !(cached.Value is SocketUserMessage msg)
				|| !(GetUserXpInformation(msg.Author.Id) is UserExperienceInformation info)
				|| !(info.RemoveMessageHash(cached.Id) is MessageHash hash)
				|| hash.MessageId == 0)
			{
				return Task.CompletedTask;
			}
			info.RemoveExperience(msg, hash.ExperienceGiven);
			DatabaseWrapper.ExecuteQuery(DatabaseQuery<UserExperienceInformation>.Update(new[] { info }));
			UpdateUserRank(info, ((SocketTextChannel)msg.Channel).Guild);
			return Task.CompletedTask;
		}
		/// <inheritdoc />
		public int CalculateLevel(int experience)
		{
			//No negative levels
			return Math.Min((int)Math.Pow(Math.Log(experience, Log), Pow), 0);
		}
		/// <inheritdoc />
		public (int Rank, int TotalUsers) GetGuildRank(SocketGuild guild, ulong userId) => GetRank(guild.Id.ToString(), userId);
		/// <inheritdoc />
		public (int Rank, int TotalUsers) GetGlobalRank(ulong userId) => GetRank("Global", userId);
		/// <inheritdoc />
		public IUserExperienceInformation GetUserXpInformation(ulong userId)
		{
			var values = DatabaseWrapper.ExecuteQuery(DatabaseQuery<UserExperienceInformation>.Get(x => x.UserId == userId, 1));
			if (!values.Any())
			{
				var value = new UserExperienceInformation(userId);
				DatabaseWrapper.ExecuteQuery(DatabaseQuery<UserExperienceInformation>.Insert(new[] { value }));
				return value;
			}
			return values.Single();
		}
		/// <inheritdoc />
		public EmbedWrapper GetUserXpInformationEmbedWrapper(SocketGuild guild, ulong userId, bool global)
		{
			var info = GetUserXpInformation(userId);
			var (rank, totalUsers) = global ? GetGlobalRank(userId) : GetGuildRank(guild, userId);
			var experience = global ? info.GetExperience() : info.GetExperience(guild);
			var level = CalculateLevel(experience);

			var user = guild.GetUser(userId);
			var name = user?.Format() ?? userId.ToString();
			var description = $"!!!TEMP PLACEHOLDER!!!\n\nRank: {rank} out of {totalUsers}\nXP: {experience}\nLevel: {level}";

			//TODO: implement rest of embed
			return new EmbedWrapper
			{
				Title = $"{(global ? "Global" : "Guild")} xp information for {name}",
				Description = description,
				ThumbnailUrl = user?.GetAvatarUrl(),
				Author = user?.CreateAuthor(),
				Footer = new EmbedFooterBuilder { Text = "Xp Information", },
			};
		}
		private void UpdateUserRank(IUserExperienceInformation info, SocketGuild guild)
		{
			UpsertRank(guild.Id.ToString(), info.UserId, info.GetExperience(guild));
			UpsertRank("Global", info.UserId, info.GetExperience());
		}
		private (int Rank, int TotalUsers) GetRank(string collectionName, ulong userId)
		{
			var options = DatabaseQuery<LeaderboardPosition>.GetAll();
			options.CollectionName = collectionName;
			var all = DatabaseWrapper.ExecuteQuery(options).OrderByDescending(x => x.Experience);
			var rank = -1;
			var total = 0;
			foreach (var entry in all)
			{
				++total;
				if (rank == -1 && entry.UserId == userId)
				{
					rank = total;
				}
			}
			return (rank, total);
		}
		private void UpsertRank(string collectionName, ulong userId, int experience)
		{
			var deleteQuery = DatabaseQuery<LeaderboardPosition>.Delete(x => x.UserId == userId);
			deleteQuery.CollectionName = collectionName;
			DatabaseWrapper.ExecuteQuery(deleteQuery);
			var insertQuery = DatabaseQuery<LeaderboardPosition>.Insert(new[] { new LeaderboardPosition(userId, experience) });
			insertQuery.CollectionName = collectionName;
			DatabaseWrapper.ExecuteQuery(insertQuery);
		}
	}
}