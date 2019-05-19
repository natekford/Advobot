using Advobot.Classes;
using Advobot.Classes.Results;
using Advobot.Classes.Settings;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.WebSocket;

namespace Advobot.Commands.Responses
{
	public sealed class GuildSettings : CommandResponses
	{
		private GuildSettings() { }

		public static AdvobotResult DisplayNames(ISettingsBase settings)
		{
			return Success(new EmbedWrapper
			{
				Title = Title.FormatInterpolated($"{settings.GetType().Name}"),
				Description = Default.FormatInterpolated($"{settings.SettingNames}"),
			});
		}
		public static AdvobotResult DisplaySettings(BaseSocketClient client, SocketGuild guild, ISettingsBase settings)
		{
			return Success(new TextFileInfo
			{
				Name = settings.GetType().Name.FormatTitle().Replace(' ', '_'),
				Text = settings.Format(client, guild),
			});
		}
		public static AdvobotResult DisplaySetting(BaseSocketClient client, SocketGuild guild, ISettingsBase settings, string name)
		{
			//TODO: make into precondition?
			if (!settings.SettingNames.CaseInsContains(name))
			{
				return Failure(Default.FormatInterpolated($"{name} is not a valid setting.")).WithTime(DefaultTime);
			}

			var description = settings.FormatSetting(client, guild, name);
			if (description.Length <= EmbedBuilder.MaxDescriptionLength)
			{
				return Success(new EmbedWrapper
				{
					Title = name,
					Description = description,
				});
			}
			return Success(new TextFileInfo
			{
				Name = name,
				Text = description,
			});
		}
		public static AdvobotResult GetFile(ISettingsBase settings, IBotDirectoryAccessor accessor)
		{
			var file = settings.GetFile(accessor);
			if (!file.Exists)
			{
				return Failure("The settings file does not exist.").WithTime(DefaultTime);
			}
			return Success(new TextFileInfo
			{
				Name = file.Name,
				Text = System.IO.File.ReadAllText(file.FullName),
			});
		}

		public static AdvobotResult SendWelcomeNotification(GuildNotification? notif)
			=> SendNotification(notif, "welcome");
		public static AdvobotResult SendGoodbyeNotification(GuildNotification? notif)
			=> SendNotification(notif, "goodbye");

		private static AdvobotResult SendNotification(GuildNotification? notif, string notifName)
		{
			if (notif == null)
			{
				return Failure($"The {notifName} notification does not exist.").WithTime(DefaultTime);
			}
			return Success(notif.Content ?? Constants.ZERO_LENGTH_CHAR)
				.WithEmbed(notif.CustomEmbed?.BuildWrapper())
				.WithOverrideDestinationChannelId(notif.ChannelId);
		}
	}
}
