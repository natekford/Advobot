﻿using Advobot.Logging.ReadOnlyModels;
using Advobot.Logging.Utilities;
using Advobot.Modules;
using Advobot.Utilities;

using Discord;

using static Advobot.Logging.Resources.Responses;

namespace Advobot.Logging.Responses
{
	public sealed class Notifications : AdvobotResult
	{
		private Notifications() : base(null, "")
		{
		}

		public static AdvobotResult Default(Notification notif)
		{
			return Success(NotificationDefault.Format(
				notif.ToString().WithBlock()
			));
		}

		public static AdvobotResult Disabled(Notification notif)
		{
			return Success(NotificationDisabled.Format(
				notif.ToString().WithBlock()
			));
		}

		public static AdvobotResult ModifiedChannel(Notification notif, ITextChannel channel)
		{
			return Success(NotificationModifedChannel.Format(
				notif.ToString().WithBlock(),
				channel.Format().WithBlock()
			));
		}

		public static AdvobotResult ModifiedContent(Notification notif, string? content)
		{
			return Success(NotificationModifiedContent.Format(
				notif.ToString().WithBlock(),
				(content ?? VariableNothing).WithBlock()
			));
		}

		public static AdvobotResult ModifiedEmbed(Notification notif, IReadOnlyCustomEmbed? embed)
		{
			var response = Success(NotificationModifiedEmbed.Format(
				notif.ToString().WithBlock()
			));
			if (embed?.HasAtleastOneNonNullProperty() == true)
			{
				response.WithEmbed(embed.BuildWrapper());
			}
			return response;
		}

		public static AdvobotResult SendNotification(
			Notification notif,
			IReadOnlyCustomNotification? notification)
		{
			if (notification == null)
			{
				return Failure(NotificationSendNull.Format(
					notif.ToString().WithBlock()
				));
			}
			else if (notification.ChannelId == 0)
			{
				return Failure(NotificationSendNoChannel.Format(
					notif.ToString().WithBlock()
				));
			}

			var response = Success(notification.Content ?? Constants.ZERO_LENGTH_CHAR)
				.WithOverrideDestinationChannelId(notification.ChannelId);
			if (notification.HasAtleastOneNonNullProperty())
			{
				response.WithEmbed(notification.BuildWrapper());
			}
			return response;
		}
	}
}