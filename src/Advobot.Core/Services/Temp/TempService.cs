using System.Linq;
using System.Threading.Tasks;

using Advobot.Classes;
using Advobot.Services.GuildSettings;
using Advobot.Services.GuildSettings.UserInformation;
using Advobot.Services.Time;
using Advobot.Services.Timers;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.WebSocket;

namespace Advobot.Services.Temp
{
	internal sealed class TempService
	{
		private static readonly PunishmentArgs _BannedNameArgs = new PunishmentArgs
		{
			Options = _BannedNameOptions,
		};

		private static readonly RequestOptions _BannedNameOptions = new RequestOptions
		{
			AuditLogReason = "Banned name"
		};

		private static readonly RequestOptions _BannedPhraseOptions = new RequestOptions
		{
			AuditLogReason = "Banned phrase"
		};

		private static readonly RequestOptions _ChannelSettingsOptions = new RequestOptions
		{
			AuditLogReason = "Channel setting"
		};

		private static readonly RequestOptions _PersistentRolesOptions = new RequestOptions
		{
			AuditLogReason = "Persistent roles"
		};

		private readonly IGuildSettingsFactory _SettingsFactory;
		private readonly ITime _Time;
		private readonly ITimerService _Timers;

		public TempService(
			BaseSocketClient client,
			IGuildSettingsFactory settingsFactory,
			ITime time,
			ITimerService timers)
		{
			_SettingsFactory = settingsFactory;
			_Time = time;
			_Timers = timers;

			client.MessageReceived += HandleMessageReceived;
			client.UserJoined += HandleUserJoined;
		}

		private async Task HandleMessageReceived(SocketMessage message)
		{
			var guild = ((IGuildChannel)message.Channel).Guild;
			var settings = await _SettingsFactory.GetOrCreateAsync(guild).CAF();
			var user = (IGuildUser)message.Author;
			var bot = await guild.GetCurrentUserAsync().CAF();

			//Ignore admins and messages older than an hour.
			if (user.GuildPermissions.Administrator
				|| (_Time.UtcNow - message.CreatedAt.UtcDateTime).Hours > 0)
			{
				return;
			}

			//Spam prevention
			if (bot.CanModify(user))
			{
				foreach (var antiSpam in settings.SpamPrevention)
				{
					await antiSpam.PunishAsync(message).CAF();
				}
			}

			//Banned phrases
			if (!settings.GetBannedPhraseUsers().TryGetSingle(x => x.UserId == user.Id, out var info))
			{
				settings.GetBannedPhraseUsers().Add(info = new BannedPhraseUserInfo(_Time, user));
			}
			if (settings.BannedPhraseStrings.TryGetFirst(x => message.Content.CaseInsContains(x.Phrase), out var str))
			{
				await str.PunishAsync(settings, guild, info, _Timers).CAF();
			}
			if (settings.BannedPhraseRegex.TryGetFirst(x => RegexUtils.IsMatch(message.Content, x.Phrase), out var regex))
			{
				await regex.PunishAsync(settings, guild, info, _Timers).CAF();
			}

			if (str != null || regex != null)
			{
				await message.DeleteAsync(_BannedPhraseOptions).CAF();
			}
			//Channel options
			else if (settings.ImageOnlyChannels.Contains(message.Channel.Id)
				&& !message.Attachments.Any(x => x.Height != null || x.Width != null)
				&& !message.Embeds.Any(x => x.Image != null))
			{
				await message.DeleteAsync(_ChannelSettingsOptions).CAF();
			}
		}

		private async Task HandleUserJoined(SocketGuildUser user)
		{
			var settings = await _SettingsFactory.GetOrCreateAsync(user.Guild).CAF();

			//Banned names
			if (settings.BannedPhraseNames.Any(x => x.Phrase.CaseInsEquals(user.Username)))
			{
				var punisher = new PunishmentManager(user.Guild, null);
				await punisher.BanAsync(user.AsAmbiguous(), _BannedNameArgs).CAF();
			}
			//Antiraid
			foreach (var antiRaid in settings.RaidPrevention)
			{
				await antiRaid.PunishAsync(user).CAF();
			}
			//Persistent roles
			var roles = settings.PersistentRoles
				.Where(x => x.UserId == user.Id)
				.Select(x => user.Guild.GetRole(x.RoleId))
				.Where(x => x != null).ToArray();
			if (roles.Length > 0)
			{
				await user.AddRolesAsync(roles, _PersistentRolesOptions).CAF();
			}
		}
	}
}