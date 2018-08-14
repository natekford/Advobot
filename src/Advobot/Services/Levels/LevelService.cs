using System;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.WebSocket;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Services.Levels
{
	/// <summary>
	/// Service for giving people experience for chatting and rewards for certain levels.
	/// </summary>
	internal sealed class LevelService : ILevelService, IUsesDatabase, IDisposable
	{
		private LiteDatabase _Db;
		private readonly DiscordShardedClient _Client;
		private readonly IGuildSettingsFactory _GuildSettings;
		private readonly IBotSettings _Settings;
		private readonly double _Log;
		private readonly double _Pow;
		private readonly TimeSpan _Time;
		private readonly int _BaseExperience;

		/// <summary>
		/// Creates an instance of <see cref="LevelService"/>.
		/// </summary>
		/// <param name="provider"></param>
		/// <param name="args"></param>
		public LevelService(IIterableServiceProvider provider, LevelServiceArguments args)
		{
			_Client = provider.GetRequiredService<DiscordShardedClient>();
			_GuildSettings = provider.GetRequiredService<IGuildSettingsFactory>();
			_Settings = provider.GetRequiredService<IBotSettings>();
			_Log = args.Log;
			_Pow = args.Pow;
			_Time = args.Time;
			_BaseExperience = args.BaseExperience;

			_Client.MessageReceived += AddExperienceAsync;
			_Client.MessageDeleted += RemoveExperienceAsync;
		}

		/// <inheritdoc />
		public void Start()
		{
			//Use mode=exclusive to not have ioexceptions
			_Db = new LiteDatabase(new ConnectionString
			{
				Filename = _Settings.GetBaseBotDirectoryFile("LevelDatabase.db").FullName,
				Mode = FileMode.Exclusive,
			}, new BsonMapper { IncludeNonPublic = true, });
			ConsoleUtils.DebugWrite($"Started the database connection for {nameof(LevelService)}.");
		}
		/// <inheritdoc />
		public void Dispose()
		{
			_Db?.Dispose();
		}
		/// <inheritdoc />
		public async Task AddExperienceAsync(SocketMessage message)
		{
			if (message.Author.IsBot
				|| message.Author.IsWebhook
				|| !(message is SocketUserMessage msg)
				|| !(await _GuildSettings.GetOrCreateAsync((msg.Channel as SocketTextChannel)?.Guild).CAF() is IGuildSettings settings)
				|| settings.IgnoredXpChannels.Contains(message.Channel.Id)
				|| !(GetUserXpInformation(message.Author.Id) is UserExperienceInformation info)
				|| info.Time > (DateTime.UtcNow - _Time))
			{
				return;
			}
			info.AddExperience(settings, msg, _BaseExperience);
			_Db.GetCollection<UserExperienceInformation>().Update(info);
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
			_Db.GetCollection<UserExperienceInformation>().Update(info);
			UpdateUserRank(info, ((SocketTextChannel)msg.Channel).Guild);
			return Task.CompletedTask;
		}
		/// <inheritdoc />
		public int CalculateLevel(int experience)
		{
			//No negative levels
			return Math.Min((int)Math.Pow(Math.Log(experience, _Log), _Pow), 0);
		}
		/// <inheritdoc />
		public (int Rank, int TotalUsers) GetGuildRank(SocketGuild guild, ulong userId)
		{
			return GetRank(_Db.GetCollection<LeaderboardPosition>(guild.Id.ToString()), userId);
		}
		/// <inheritdoc />
		public (int Rank, int TotalUsers) GetGlobalRank(ulong userId)
		{
			return GetRank(_Db.GetCollection<LeaderboardPosition>("Global"), userId);
		}
		/// <inheritdoc />
		public IUserExperienceInformation GetUserXpInformation(ulong userId)
		{
			var col = _Db.GetCollection<UserExperienceInformation>();
			//Cannot use FindById because key gets converted to double and loses precision.
			//Can cast to decimal to avoid that, but would rather not in case there are other issues with that.
			if (!(col.FindOne(x => x.UserId == userId) is UserExperienceInformation info))
			{
				col.Insert(info = new UserExperienceInformation(userId));
			}
			return info;
		}
		/// <inheritdoc />
		public async Task SendUserXpInformationAsync(SocketTextChannel channel, ulong userId, bool global)
		{
			var info = GetUserXpInformation(userId);
			var (rank, totalUsers) = global ? GetGlobalRank(userId) : GetGuildRank(channel.Guild, userId);
			var experience = global ? info.Experience : info.GetExperience(channel.Guild);
			var level = CalculateLevel(experience);

			var name = userId.ToString();
			var pfp = default(string);
			var user = channel.Guild.GetUser(userId);
			if (user != null)
			{
				name = user.Format();
				pfp = user.GetDefaultAvatarUrl();
			}

			//TODO: implement rest of embed
			var embed = new EmbedWrapper
			{
				Title = $"{(global ? "Global" : "Guild")} xp information for {name}",
				ThumbnailUrl = pfp,
			};
			embed.TryAddAuthor(user, out _);
			embed.TryAddFooter("Xp Information", null, out _);
			await MessageUtils.SendMessageAsync(channel, null, embed).CAF();
		}
		private void UpdateUserRank(IUserExperienceInformation info, SocketGuild guild)
		{
			_Db.GetCollection<LeaderboardPosition>(guild.Id.ToString()).Upsert(new LeaderboardPosition(info.UserId, info.GetExperience(guild)));
			_Db.GetCollection<LeaderboardPosition>("Global").Upsert(new LeaderboardPosition(info.UserId, info.Experience));
		}
		private (int Rank, int TotalUsers) GetRank(LiteCollection<LeaderboardPosition> col, ulong userId)
		{
			var all = col.Find(Query.All(nameof(LeaderboardPosition.Experience), Query.Descending));
			//Only iterate through 
			var rank = -1;
			var total = -1;
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
	}
}