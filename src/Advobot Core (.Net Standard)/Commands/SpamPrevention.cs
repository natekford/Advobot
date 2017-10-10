using Advobot.Actions;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.SpamPrevention;
using Advobot.Enums;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Commands.SpamPrevention
{
	[Group(nameof(PreventSpam)), TopLevelShortAlias(typeof(PreventSpam))]
	[Summary("Spam prevention allows for some protection against mention spammers. Messages are the amount of messages a user has to send with the given amount of mentions before being considered " +
		"as potential spam. Votes is the amount of users that have to agree with the potential punishment. The spam users are reset every hour. `Show` lists all of the available punishments.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	public sealed class PreventSpam : SavingModuleBase
	{
		[Command(nameof(Show)), ShortAlias(nameof(Show))]
		public async Task Show()
		{
			var desc = $"`{String.Join("`, `", Enum.GetNames(typeof(PunishmentType)))}`";
			await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Punishment Types", desc));
		}
		[Command(nameof(Create)), ShortAlias(nameof(Create))]
		public async Task Create(SpamType spamType, PunishmentType punishment, uint messageCount, uint requiredSpamAmtOrTimeInterval, uint votes)
		{
			if (!SpamPreventionInfo.TryCreateSpamPreventionInfo(spamType, punishment, (int)messageCount, (int)requiredSpamAmtOrTimeInterval, (int)votes, out var spamPrevention, out var errorReason))
			{
				await MessageActions.SendErrorMessage(Context, errorReason);
				return;
			}

			Context.GuildSettings.SpamPreventionDictionary[spamType] = spamPrevention;
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set up the spam prevention for `{spamType.EnumName().ToLower()}`.\n{spamPrevention.ToString()}");
		}
		[Command(nameof(Enable)), ShortAlias(nameof(Enable))]
		public async Task Enable(SpamType spamType)
		{
			var spamPrev = Context.GuildSettings.SpamPreventionDictionary[spamType];
			if (spamPrev == null)
			{
				await MessageActions.SendErrorMessage(Context, new ErrorReason("There must be a spam prevention of that type set up before one can be enabled or disabled."));
				return;
			}

			spamPrev.Enable();
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, "Successfully enabled the given spam prevention.");
		}
		[Command(nameof(Disable)), ShortAlias(nameof(Disable))]
		public async Task Disable(SpamType spamType)
		{
			var spamPrev = Context.GuildSettings.SpamPreventionDictionary[spamType];
			if (spamPrev == null)
			{
				await MessageActions.SendErrorMessage(Context, new ErrorReason("There must be a spam prevention of that type set up before one can be enabled or disabled."));
				return;
			}

			spamPrev.Disable();
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, "Successfully disabled the given spam prevention.");
		}
	}

	[Group(nameof(PreventRaid)), TopLevelShortAlias(typeof(PreventRaid))]
	[Summary("Any users who joins from now on will get text muted. Once `preventraidspam` is turned off all the users who were muted will be unmuted. " +
		"Inputting a number means the last x amount of people (up to 25) who have joined will be muted. `Show` lists all of the available punishments.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	public sealed class PreventRaid : SavingModuleBase
	{
		[Command(nameof(Show)), ShortAlias(nameof(Show))]
		public async Task Show()
		{
			var desc = $"`{String.Join("`, `", Enum.GetNames(typeof(PunishmentType)))}`";
			await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Punishment Types", desc));
		}
		[Command(nameof(Create)), ShortAlias(nameof(Create))]
		public async Task Create(RaidType raidType, PunishmentType punishment, uint userCount, uint interval)
		{
			if (!RaidPreventionInfo.TryCreateRaidPreventionInfo(raidType, punishment, (int)userCount, (int)interval, out var raidPrevention, out var errorReason))
			{
				await MessageActions.SendErrorMessage(Context, errorReason);
				return;
			}

			Context.GuildSettings.RaidPreventionDictionary[raidType] = raidPrevention;
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set up the raid prevention for `{raidType.EnumName().ToLower()}`.\n{raidPrevention.ToString()}");
		}
		[Command(nameof(Enable)), ShortAlias(nameof(Enable))]
		public async Task Enable(RaidType raidType)
		{
			var raidPrev = Context.GuildSettings.RaidPreventionDictionary[raidType];
			if (raidPrev == null)
			{
				await MessageActions.SendErrorMessage(Context, new ErrorReason("There must be a raid prevention of that type set up before one can be enabled or disabled."));
				return;
			}

			raidPrev.Enable();
			if (raidType == RaidType.Regular)
			{
				//Mute the newest joining users
				var users = (await GuildActions.GetUsersAndOrderByJoin(Context.Guild)).Reverse().ToArray();
				for (int i = 0; i < new[] { raidPrev.UserCount, users.Length, 25 }.Min(); ++i)
				{
					await raidPrev.RaidPreventionPunishment(Context.GuildSettings, users[i]);
				}
			}

			await MessageActions.MakeAndDeleteSecondaryMessage(Context, "Successfully enabled the given raid prevention.");
		}
		[Command(nameof(Disable)), ShortAlias(nameof(Disable))]
		public async Task Disable(RaidType raidType)
		{
			var raidPrev = Context.GuildSettings.RaidPreventionDictionary[raidType];
			if (raidPrev == null)
			{
				await MessageActions.SendErrorMessage(Context, new ErrorReason("There must be a raid prevention of that type set up before one can be enabled or disabled."));
				return;
			}

			raidPrev.Disable();
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, "Successfully disabled the given raid prevention.");
		}
	}
}
