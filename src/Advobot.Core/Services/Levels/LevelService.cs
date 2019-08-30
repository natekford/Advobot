using System;
using System.Linq;
using System.Threading.Tasks;

using Advobot.Classes;
using Advobot.Databases;
using Advobot.Databases.Abstract;
using Advobot.Services.GuildSettings;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.WebSocket;

using LiteDB;

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
		/// Base XP per message.
		/// </summary>
		private int BaseExperience { get; }

		/// <summary>
		/// The bot client.
		/// </summary>
		private BaseSocketClient Client { get; }

		/// <summary>
		/// Used in calculating XP.
		/// </summary>
		private double Log { get; }

		/// <summary>
		/// Used in calculating XP.
		/// </summary>
		private double Pow { get; }

		/// <summary>
		/// The settings for each individual guild.
		/// </summary>
		private IGuildSettingsFactory SettingsFactory { get; }

		/// <summary>
		/// The time between when XP can be gained again.
		/// </summary>
		private TimeSpan Time { get; }

		/// <summary>
		/// Creates an instance of <see cref="LevelService"/>.
		/// </summary>
		/// <param name="dbFactory"></param>
		/// <param name="settingsFactory"></param>
		/// <param name="client"></param>
		public LevelService(
			IDatabaseWrapperFactory dbFactory,
			IGuildSettingsFactory settingsFactory,
			BaseSocketClient client)
			: this(dbFactory, settingsFactory, client, new LevelServiceArguments()) { }

		/// <summary>
		/// Creates an instance of <see cref="LevelService"/>.
		/// </summary>
		/// <param name="dbFactory"></param>
		/// <param name="settingsFactory"></param>
		/// <param name="client"></param>
		/// <param name="args"></param>
		public LevelService(
			IDatabaseWrapperFactory dbFactory,
			IGuildSettingsFactory settingsFactory,
			BaseSocketClient client,
			LevelServiceArguments args)
			: base(dbFactory)
		{
			Client = client;
			SettingsFactory = settingsFactory;
			Log = args.Log;
			Pow = args.Pow;
			Time = args.Time;
			BaseExperience = args.BaseExperience;

			Client.MessageReceived += AddExperienceAsync;
			Client.MessageDeleted += (cached, _) => RemoveExperienceAsync(cached);
		}

		/// <inheritdoc />
		public async Task AddExperienceAsync(IMessage message)
		{
			if (message.Author.IsBot
				|| message.Author.IsWebhook
				|| !(message is IUserMessage msg)
				|| !(msg.Channel is ITextChannel channel)
				|| !(channel.Guild is IGuild guild)
				|| !(await SettingsFactory.GetOrCreateAsync(guild).CAF() is IGuildSettings settings)
				|| settings.IgnoredXpChannels.Contains(msg.Channel.Id)
				|| !(GetUserXpInformation(message.Author) is UserExperienceInformation info)
				|| info.Time > (DateTime.UtcNow - Time))
			{
				return;
			}

			info.AddExperience(msg, BaseExperience);
			DatabaseWrapper.ExecuteQuery(DatabaseQuery<UserExperienceInformation>.Update(new[] { info }));
			UpdateUserRank(info, guild);
		}

		/// <inheritdoc />
		public int CalculateLevel(int experience)
		{
			var logged = Math.Log(experience, Log);
			var powed = (int)Math.Pow(logged, Pow);
			return Math.Min(powed, 0); //No negative levels
		}

		/// <inheritdoc />
		public (int Rank, int TotalUsers) GetGlobalRank(IUser user)
			=> GetRank("Global", user.Id);

		/// <inheritdoc />
		public (int Rank, int TotalUsers) GetGuildRank(IGuild guild, IUser user)
			=> GetRank(GetCollectionName(guild), user.Id);

		/// <inheritdoc />
		public IUserExperienceInformation GetUserXpInformation(IUser user)
		{
			var values = DatabaseWrapper.ExecuteQuery(DatabaseQuery<UserExperienceInformation>.Get(x => x.Id == user.Id, 1));
			if (!values.Any())
			{
				var value = new UserExperienceInformation(user);
				DatabaseWrapper.ExecuteQuery(DatabaseQuery<UserExperienceInformation>.Insert(new[] { value }));
				return value;
			}
			return values.Single();
		}

		/// <inheritdoc />
		public EmbedWrapper GetUserXpInformationEmbedWrapper(IGuild guild, IUser user, bool global)
		{
			var info = GetUserXpInformation(user);
			var (rank, totalUsers) = global ? GetGlobalRank(user) : GetGuildRank(guild, user);
			var experience = global ? info.GetExperience() : info.GetExperience(guild);
			var level = CalculateLevel(experience);

			var name = user.Format() ?? user.Id.ToString();
			var description = $"Rank: {rank} out of {totalUsers}\nXP: {experience}\nLevel: {level}";
			return new EmbedWrapper
			{
				Title = $"{(global ? "Global" : "Guild")} xp information for {name}",
				Description = description,
				ThumbnailUrl = user.GetAvatarUrl(),
				Author = user.CreateAuthor(),
				Footer = new EmbedFooterBuilder { Text = "Xp Information", },
			};
		}

		/// <inheritdoc />
		public async Task RemoveExperienceAsync(Cacheable<IMessage, ulong> cached)
		{
			if (!cached.HasValue
				|| cached.Value.Author.IsBot
				|| cached.Value.Author.IsWebhook
				|| !(cached.Value is IUserMessage msg)
				|| !(msg.Channel is ITextChannel channel)
				|| !(channel.Guild is IGuild guild)
				|| !(await SettingsFactory.GetOrCreateAsync(guild).CAF() is IGuildSettings settings)
				|| settings.IgnoredXpChannels.Contains(msg.Channel.Id)
				|| !(GetUserXpInformation(msg.Author) is UserExperienceInformation info)
				|| !(info.RemoveMessageHash(cached.Id) is MessageHash hash)
				|| hash.MessageId == 0)
			{
				return;
			}

			info.RemoveExperience(msg, hash.ExperienceGiven);
			DatabaseWrapper.ExecuteQuery(DatabaseQuery<UserExperienceInformation>.Update(new[] { info }));
			UpdateUserRank(info, guild);
		}

		protected override void AfterStart(int schema)
		{
			if (schema < 3) //Relying on LiteDB
			{
				var db = (LiteDatabase)DatabaseWrapper.UnderlyingDatabase;
				var notWanted = new[] { "UserExperienceInformation", "Meta" };
				var colNames = db.GetCollectionNames().Where(x => !notWanted.Contains(x));
				foreach (var colName in colNames)
				{
					var col = db.GetCollection(colName);
					foreach (var doc in col.FindAll())
					{
						doc["_id"] = doc["UserId"];
						doc.Remove("UserId");
						col.Update(doc);
					}
				}
			}
		}

		private string GetCollectionName(IGuild guild)
		{
			//LiteDB doesn't support numbers anymore (since 5.0 preview)
			var name = "";
			var id = guild.Id;
			while (--id != ulong.MaxValue)
			{
				name = (char)('A' + (id % 26)) + name;
				id /= 26;
			}
			return name;
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
				if (rank == -1 && entry.Id == userId)
				{
					rank = total;
				}
			}
			return (rank, total);
		}

		private void UpdateRank(string collectionName, ulong userId, int experience)
		{
			var insertQuery = DatabaseQuery<LeaderboardPosition>.Update(new[] { new LeaderboardPosition(userId, experience) });
			insertQuery.CollectionName = collectionName;
			DatabaseWrapper.ExecuteQuery(insertQuery);
		}

		private void UpdateUserRank(IUserExperienceInformation info, IGuild guild)
		{
			UpdateRank(GetCollectionName(guild), info.UserId, info.GetExperience(guild));
			UpdateRank("Global", info.UserId, info.GetExperience());
		}
	}
}