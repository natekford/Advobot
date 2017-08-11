using Advobot.Actions;
using Advobot.Attributes;
using Advobot.Enums;
using Advobot.NonSavedClasses;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Advobot
{
	namespace SpamPrevention
	{
		[Group("preventspam"), Alias("prs")]
		[Usage("[Message|LongMessage|Link|Image|Mention|ShowPunishments] <Setup|On|Off> <Punishment> <Message Count> <Spam Amount|Time Interval> <Votes>")]
		[Summary("Spam prevention allows for some protection against mention spammers. Messages are the amount of messages a user has to send with the given amount of mentions before being considered " +
			"as potential spam. Votes is the amount of users that have to agree with the potential punishment. The spam users are reset every hour.")]
		[PermissionRequirement(null, null)]
		[DefaultEnabled(false)]
		public sealed class PreventSpam : MySavingModuleBase
		{
			//idk exactly if it's a good idea to be using nested classes. shouldn't be that hard to change them to non nested classes if need be.
			[Group("showpunishments"), Alias("show punishments")]
			public sealed class ShowPunishments : MyModuleBase
			{
				[Command]
				public async Task Command()
				{
					await CommandRunner();
				}

				private async Task CommandRunner()
				{
					var desc = String.Format("`{0}`", String.Join("`, `", Enum.GetNames(typeof(PunishmentType))));
					await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Punishment Types", desc));
				}
			}

			[Group(nameof(SpamType.Message)), Alias("msg")]
			public sealed class PreventMessageSpam : MySavingModuleBase
			{
				[Command("on")]
				public async Task CommandOn()
				{
					await SpamActions.ModifySpamPreventionEnabled(Context, SpamType.Message, true);
				}
				[Command("off")]
				public async Task CommandOff()
				{
					await SpamActions.ModifySpamPreventionEnabled(Context, SpamType.Message, false);
				}
				[Command("setup")]
				public async Task CommandSetup(PunishmentType punishment, uint messageCount, uint requiredSpamAmtOrTimeInterval, uint votes)
				{
					await SpamActions.SetUpSpamPrevention(Context, SpamType.Message, punishment, messageCount, requiredSpamAmtOrTimeInterval, votes);
				}
			}

			[Group(nameof(SpamType.LongMessage)), Alias("long message", "lmsg")]
			public sealed class PreventLongMessageSpam : MySavingModuleBase
			{
				[Command("on")]
				public async Task CommandOn()
				{
					await SpamActions.ModifySpamPreventionEnabled(Context, SpamType.LongMessage, true);
				}
				[Command("off")]
				public async Task CommandOff()
				{
					await SpamActions.ModifySpamPreventionEnabled(Context, SpamType.LongMessage, false);
				}
				[Command("setup")]
				public async Task CommandSetup(PunishmentType punishment, uint messageCount, uint requiredSpamAmtOrTimeInterval, uint votes)
				{
					await SpamActions.SetUpSpamPrevention(Context, SpamType.LongMessage, punishment, messageCount, requiredSpamAmtOrTimeInterval, votes);
				}
			}

			[Group(nameof(SpamType.Link)), Alias("l")]
			public sealed class PreventLinkSpam : MySavingModuleBase
			{
				[Command("on")]
				public async Task CommandOn()
				{
					await SpamActions.ModifySpamPreventionEnabled(Context, SpamType.Link, true);
				}
				[Command("off")]
				public async Task CommandOff()
				{
					await SpamActions.ModifySpamPreventionEnabled(Context, SpamType.Link, false);
				}
				[Command("setup")]
				public async Task CommandSetup(PunishmentType punishment, uint messageCount, uint requiredSpamAmtOrTimeInterval, uint votes)
				{
					await SpamActions.SetUpSpamPrevention(Context, SpamType.Link, punishment, messageCount, requiredSpamAmtOrTimeInterval, votes);
				}
			}

			[Group(nameof(SpamType.Image)), Alias("img")]
			public sealed class PreventImageSpam : MySavingModuleBase
			{
				[Command("on")]
				public async Task CommandOn()
				{
					await SpamActions.ModifySpamPreventionEnabled(Context, SpamType.Image, true);
				}
				[Command("off")]
				public async Task CommandOff()
				{
					await SpamActions.ModifySpamPreventionEnabled(Context, SpamType.Image, false);
				}
				[Command("setup")]
				public async Task CommandSetup(PunishmentType punishment, uint messageCount, uint requiredSpamAmtOrTimeInterval, uint votes)
				{
					await SpamActions.SetUpSpamPrevention(Context, SpamType.Image, punishment, messageCount, requiredSpamAmtOrTimeInterval, votes);
				}
			}

			[Group(nameof(SpamType.Mention)), Alias("men")]
			public sealed class PreventMentionSpam : MySavingModuleBase
			{
				[Command("on")]
				public async Task CommandOn()
				{
					await SpamActions.ModifySpamPreventionEnabled(Context, SpamType.Mention, true);
				}
				[Command("off")]
				public async Task CommandOff()
				{
					await SpamActions.ModifySpamPreventionEnabled(Context, SpamType.Mention, false);
				}
				[Command("setup")]
				public async Task CommandSetup(PunishmentType punishment, uint messageCount, uint requiredSpamAmtOrTimeInterval, uint votes)
				{
					await SpamActions.SetUpSpamPrevention(Context, SpamType.Mention, punishment, messageCount, requiredSpamAmtOrTimeInterval, votes);
				}
			}
		}

		[Group("preventraid")]
		[Alias("prr")]
		[Usage("[Regular|RapidJoins|ShowPunishments] <Setup|On|Off> <Punishment> <Number of Users> <Time Interval>")]
		[Summary("Any users who joins from now on will get text muted. Once `preventraidspam` is turned off all the users who were muted will be unmuted. " +
			"Inputting a number means the last x amount of people (up to 25) who have joined will be muted.")]
		[PermissionRequirement(null, null)]
		[DefaultEnabled(false)]
		public sealed class PreventRaid : MySavingModuleBase
		{
			[Group("showpunishments"), Alias("show punishments")]
			public sealed class ShowPunishments : MyModuleBase
			{
				[Command]
				public async Task Command()
				{
					await CommandRunner();
				}

				private async Task CommandRunner()
				{
					var desc = String.Format("`{0}`", String.Join("`, `", Enum.GetNames(typeof(PunishmentType))));
					await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Punishment Types", desc));
				}
			}

			[Group(nameof(RaidType.Regular)), Alias("reg")]
			public sealed class PreventRegularRaid : MySavingModuleBase
			{
				[Command("on")]
				public async Task CommandOn()
				{
					await SpamActions.ModifyRaidPreventionEnabled(Context, RaidType.Regular, true);
				}
				[Command("off")]
				public async Task CommandOff()
				{
					await SpamActions.ModifyRaidPreventionEnabled(Context, RaidType.Regular, false);
				}
				[Command("setup")]
				public async Task CommandSetup(PunishmentType punishment, uint numberOfUsers)
				{
					await SpamActions.SetUpRaidPrevention(Context, RaidType.Regular, punishment, numberOfUsers, 0);
				}
			}

			[Group(nameof(RaidType.RapidJoins)), Alias("rapid joins", "joins")]
			public sealed class PreventRapidJoinsRaid : MySavingModuleBase
			{
				[Command("on")]
				public async Task CommandOn()
				{
					await SpamActions.ModifyRaidPreventionEnabled(Context, RaidType.RapidJoins, true);
				}
				[Command("off")]
				public async Task CommandOff()
				{
					await SpamActions.ModifyRaidPreventionEnabled(Context, RaidType.RapidJoins, false);
				}
				[Command("setup")]
				public async Task CommandSetup(PunishmentType punishment, uint numberOfUsers, uint interval)
				{
					await SpamActions.SetUpRaidPrevention(Context, RaidType.RapidJoins, punishment, numberOfUsers, interval);
				}
			}
		}
	}
}
