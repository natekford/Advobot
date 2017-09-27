using Advobot.Actions;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Enums;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Advobot.Commands.SpamPrevention
{
	[Group(nameof(PreventSpam)), Alias("prs")]
	[Usage("[Message|LongMessage|Link|Image|Mention|Show] <Setup|On|Off> <Punishment> <Message Count> <Spam Amount|Time Interval> <Votes>")]
	[Summary("Spam prevention allows for some protection against mention spammers. Messages are the amount of messages a user has to send with the given amount of mentions before being considered " +
		"as potential spam. Votes is the amount of users that have to agree with the potential punishment. The spam users are reset every hour. `Show` lists all of the available punishments.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	public sealed class PreventSpam : MySavingModuleBase
	{
		[Group(nameof(ActionType.Show)), Alias("s")]
		public sealed class ShowPunishments : MyModuleBase
		{
			[Command]
			public async Task Command()
			{
				var desc = $"`{String.Join("`, `", Enum.GetNames(typeof(PunishmentType)))}`";
				await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Punishment Types", desc));
			}
		}

		[Group(nameof(SpamType.Message)), Alias("msg")]
		public sealed class PreventMessageSpam : MySavingModuleBase
		{
			private const SpamType _SpamType = SpamType.Message;

			[Command(nameof(ActionType.On))]
			public async Task CommandOn()
			{
				await SpamPreventionActions.ModifySpamPreventionEnabled(Context, _SpamType, true);
			}
			[Command(nameof(ActionType.Off))]
			public async Task CommandOff()
			{
				await SpamPreventionActions.ModifySpamPreventionEnabled(Context, _SpamType, false);
			}
			[Command(nameof(ActionType.Setup))]
			public async Task CommandSetup(PunishmentType punishment, uint messageCount, uint requiredSpamAmtOrTimeInterval, uint votes)
			{
				await SpamPreventionActions.SetUpSpamPrevention(Context, _SpamType, punishment, messageCount, requiredSpamAmtOrTimeInterval, votes);
			}
		}

		[Group(nameof(SpamType.LongMessage)), Alias("long message", "lmsg")]
		public sealed class PreventLongMessageSpam : MySavingModuleBase
		{
			private const SpamType _SpamType = SpamType.LongMessage;

			[Command(nameof(ActionType.On))]
			public async Task CommandOn()
			{
				await SpamPreventionActions.ModifySpamPreventionEnabled(Context, _SpamType, true);
			}
			[Command(nameof(ActionType.Off))]
			public async Task CommandOff()
			{
				await SpamPreventionActions.ModifySpamPreventionEnabled(Context, _SpamType, false);
			}
			[Command(nameof(ActionType.Setup))]
			public async Task CommandSetup(PunishmentType punishment, uint messageCount, uint requiredSpamAmtOrTimeInterval, uint votes)
			{
				await SpamPreventionActions.SetUpSpamPrevention(Context, _SpamType, punishment, messageCount, requiredSpamAmtOrTimeInterval, votes);
			}
		}

		[Group(nameof(SpamType.Link)), Alias("l")]
		public sealed class PreventLinkSpam : MySavingModuleBase
		{
			private const SpamType _SpamType = SpamType.Link;

			[Command(nameof(ActionType.On))]
			public async Task CommandOn()
			{
				await SpamPreventionActions.ModifySpamPreventionEnabled(Context, _SpamType, true);
			}
			[Command(nameof(ActionType.Off))]
			public async Task CommandOff()
			{
				await SpamPreventionActions.ModifySpamPreventionEnabled(Context, _SpamType, false);
			}
			[Command(nameof(ActionType.Setup))]
			public async Task CommandSetup(PunishmentType punishment, uint messageCount, uint requiredSpamAmtOrTimeInterval, uint votes)
			{
				await SpamPreventionActions.SetUpSpamPrevention(Context, _SpamType, punishment, messageCount, requiredSpamAmtOrTimeInterval, votes);
			}
		}

		[Group(nameof(SpamType.Image)), Alias("img")]
		public sealed class PreventImageSpam : MySavingModuleBase
		{
			private const SpamType _SpamType = SpamType.Image;

			[Command(nameof(ActionType.On))]
			public async Task CommandOn()
			{
				await SpamPreventionActions.ModifySpamPreventionEnabled(Context, _SpamType, true);
			}
			[Command(nameof(ActionType.Off))]
			public async Task CommandOff()
			{
				await SpamPreventionActions.ModifySpamPreventionEnabled(Context, _SpamType, false);
			}
			[Command(nameof(ActionType.Setup))]
			public async Task CommandSetup(PunishmentType punishment, uint messageCount, uint requiredSpamAmtOrTimeInterval, uint votes)
			{
				await SpamPreventionActions.SetUpSpamPrevention(Context, _SpamType, punishment, messageCount, requiredSpamAmtOrTimeInterval, votes);
			}
		}

		[Group(nameof(SpamType.Mention)), Alias("men")]
		public sealed class PreventMentionSpam : MySavingModuleBase
		{
			private const SpamType _SpamType = SpamType.Mention;

			[Command(nameof(ActionType.On))]
			public async Task CommandOn()
			{
				await SpamPreventionActions.ModifySpamPreventionEnabled(Context, _SpamType, true);
			}
			[Command(nameof(ActionType.Off))]
			public async Task CommandOff()
			{
				await SpamPreventionActions.ModifySpamPreventionEnabled(Context, _SpamType, false);
			}
			[Command(nameof(ActionType.Setup))]
			public async Task CommandSetup(PunishmentType punishment, uint messageCount, uint requiredSpamAmtOrTimeInterval, uint votes)
			{
				await SpamPreventionActions.SetUpSpamPrevention(Context, _SpamType, punishment, messageCount, requiredSpamAmtOrTimeInterval, votes);
			}
		}
	}

	[Group(nameof(PreventRaid))]
	[Alias("prr")]
	[Usage("[Regular|RapidJoins|Show] <Setup|On|Off> <Punishment> <Number of Users> <Time Interval>")]
	[Summary("Any users who joins from now on will get text muted. Once `preventraidspam` is turned off all the users who were muted will be unmuted. " +
		"Inputting a number means the last x amount of people (up to 25) who have joined will be muted. `Show` lists all of the available punishments.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	public sealed class PreventRaid : MySavingModuleBase
	{
		[Group(nameof(ActionType.Show)), Alias("s")]
		public sealed class ShowPunishments : MyModuleBase
		{
			[Command]
			public async Task Command()
			{
				var desc = $"`{String.Join("`, `", Enum.GetNames(typeof(PunishmentType)))}`";
				await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Punishment Types", desc));
			}
		}

		[Group(nameof(RaidType.Regular)), Alias("reg")]
		public sealed class PreventRegularRaid : MySavingModuleBase
		{
			private const RaidType _RaidType = RaidType.Regular;

			[Command(nameof(ActionType.On))]
			public async Task CommandOn()
			{
				await SpamPreventionActions.ModifyRaidPreventionEnabled(Context, _RaidType, true);
			}
			[Command(nameof(ActionType.Off))]
			public async Task CommandOff()
			{
				await SpamPreventionActions.ModifyRaidPreventionEnabled(Context, _RaidType, false);
			}
			[Command(nameof(ActionType.Setup))]
			public async Task CommandSetup(PunishmentType punishment, uint numberOfUsers)
			{
				await SpamPreventionActions.SetUpRaidPrevention(Context, _RaidType, punishment, numberOfUsers, 0);
			}
		}

		[Group(nameof(RaidType.RapidJoins)), Alias("rapid joins", "joins")]
		public sealed class PreventRapidJoinsRaid : MySavingModuleBase
		{
			private const RaidType _RaidType = RaidType.RapidJoins;

			[Command(nameof(ActionType.On))]
			public async Task CommandOn()
			{
				await SpamPreventionActions.ModifyRaidPreventionEnabled(Context, _RaidType, true);
			}
			[Command(nameof(ActionType.Off))]
			public async Task CommandOff()
			{
				await SpamPreventionActions.ModifyRaidPreventionEnabled(Context, _RaidType, false);
			}
			[Command(nameof(ActionType.Setup))]
			public async Task CommandSetup(PunishmentType punishment, uint numberOfUsers, uint interval)
			{
				await SpamPreventionActions.SetUpRaidPrevention(Context, _RaidType, punishment, numberOfUsers, interval);
			}
		}
	}
}
