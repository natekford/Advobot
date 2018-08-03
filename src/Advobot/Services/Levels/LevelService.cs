using System;
using System.Threading.Tasks;
using Advobot.Interfaces;
using Advobot.Utilities;
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
		private readonly IGuildSettingsService _GuildSettings;
		private readonly double _Log;
		private readonly double _Pow;
		private readonly TimeSpan _Time;
		private readonly int _BaseExperience;

		/// <summary>
		/// Creates an instance of <see cref="LevelService"/>.
		/// </summary>
		/// <param name="services"></param>
		/// <param name="args"></param>
		public LevelService(IServiceProvider services, LevelServiceArguments args)
		{
			_Client = services.GetRequiredService<DiscordShardedClient>();
			_GuildSettings = services.GetRequiredService<IGuildSettingsService>();
			_Log = args.Log;
			_Pow = args.Pow;
			_Time = args.Time;
			_BaseExperience = args.BaseExperience;

			_Client.MessageReceived += AddExperience;
			_Client.MessageDeleted += RemoveExperience;
		}

		/// <inheritdoc />
		public void Start()
		{
			//Use mode=exclusive to not have ioexceptions
			_Db = new LiteDatabase(new ConnectionString
			{
				Filename = FileUtils.GetBaseBotDirectoryFile("LevelDatabase.db").FullName,
				Mode = FileMode.Exclusive,
			}, new BsonMapper { IncludeNonPublic = true, });
		}
		/// <inheritdoc />
		public void Dispose()
		{
			_Db.Dispose();
		}
		/// <inheritdoc />
		public Task AddExperience(SocketMessage message)
		{
			if (_Db == null || message.Author.IsBot || message.Author.IsWebhook || !(message is SocketUserMessage msg))
			{
				return Task.FromResult(0);
			}
			var info = (UserExperienceInformation)GetUserInformation(message.Author.Id);
			if (info.Time > (DateTime.UtcNow - _Time))
			{
				return Task.FromResult(0);
			}
			info.AddExperience(msg, _BaseExperience);
			_Db.GetCollection<UserExperienceInformation>().Update(info);
			return Task.FromResult(0);
		}
		/// <inheritdoc />
		public Task RemoveExperience(Cacheable<IMessage, ulong> cached, ISocketMessageChannel channel)
		{
			if (!cached.HasValue || !(cached.Value is SocketUserMessage message))
			{
				return Task.FromResult(0);
			}
			var info = (UserExperienceInformation)GetUserInformation(message.Author.Id);
			var hash = info.RemoveMessageHash(cached.Id);
			if (hash.MessageId == 0)
			{
				return Task.FromResult(0);
			}
			info.RemoveExperience(message, hash.ExperienceGiven);
			_Db.GetCollection<UserExperienceInformation>().Update(info);
			return Task.FromResult(0);
		}
		/// <inheritdoc />
		public int CalculateLevel(int experience)
		{
			//No negative levels
			return Math.Min((int)Math.Pow(Math.Log(experience, _Log), _Pow), 0);
		}
		/// <inheritdoc />
		public IUserExperienceInformation GetUserInformation(ulong userId)
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
	}
}