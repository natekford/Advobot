using Advobot.Logging.Database.Models;
using Advobot.Logging.Resetters;
using Advobot.Preconditions.Permissions;
using Advobot.Resources;

using YACCS.Commands.Attributes;
using YACCS.Commands.Building;
using YACCS.Localization;

namespace Advobot.Logging.Commands;

[LocalizedCategory(nameof(Names.NotificationsCategory))]
[Command(nameof(Names.Notifications), nameof(Names.NotificationsAlias))]
public sealed class Notifications
{
	[Command(nameof(Names.ModifyGoodbyeMessage), nameof(Names.ModifyGoodbyeMessageAlias))]
	[LocalizedSummary(nameof(Summaries.ModifyGoodbyeMessageSummary))]
	[Id("c59f41ec-5892-496e-beaa-eabceca4bded")]
	[RequireGuildPermissions]
	public sealed class ModifyGoodbyeMessage()
		: NotificationModuleBase(Notification.Goodbye)
	{
		[InjectService]
		public required GoodbyeNotificationResetter DefaultGoodbyeSetter { get; set; }
		public override NotificationResetter DefaultSetter => DefaultGoodbyeSetter;
	}

	[Command(nameof(Names.ModifyWelcomeMessage), nameof(Names.ModifyWelcomeMessageAlias))]
	[LocalizedSummary(nameof(Summaries.ModifyWelcomeMessageSummary))]
	[Id("e95c8444-6a9a-40e7-a287-91e59200d4b6")]
	[RequireGuildPermissions]
	public sealed class ModifyWelcomeMessage()
		: NotificationModuleBase(Notification.Welcome)
	{
		public override NotificationResetter DefaultSetter => DefaultWelcomeSetter;
		[InjectService]
		public required WelcomeNotificationResetter DefaultWelcomeSetter { get; set; }
	}
}