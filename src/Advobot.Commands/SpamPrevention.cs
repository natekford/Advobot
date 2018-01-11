using Advobot.Core.Utilities;
using Advobot.Core.Classes;
using Advobot.Core.Classes.Attributes;
using Advobot.Core.Classes.SpamPrevention;
using Advobot.Core.Enums;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Commands.SpamPrevention
{
	[Group(nameof(PreventSpam)), TopLevelShortAlias(typeof(PreventSpam))]
	[Summary("Spam prevention allows for some protection against mention spammers. " +
		"Messages is the amount of messages a user has to send with the given amount of mentions before being considered as potential spam. " +
		"Votes is the amount of users that have to agree with the potential punishment. " +
		"The spam users are reset every hour. " +
		"`Show` lists all of the available punishments.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	public sealed class PreventSpam : SavingModuleBase
	{
		[Command(nameof(Show)), ShortAlias(nameof(Show))]
		public async Task Show()
		{
			var desc = $"`{String.Join("`, `", Enum.GetNames(typeof(PunishmentType)))}`";
			await MessageUtils.SendEmbedMessageAsync(Context.Channel, new EmbedWrapper("Punishment Types", desc)).CAF();
		}
		[Command(nameof(Create)), ShortAlias(nameof(Create))]
		public async Task Create(SpamType spamType, PunishmentType punishment, uint messageCount, uint requiredSpamAmtOrTimeInterval, uint votes)
		{
			if (!SpamPreventionInfo.TryCreateSpamPreventionInfo(
				spamType,
				punishment,
				(int)messageCount,
				(int)requiredSpamAmtOrTimeInterval,
				(int)votes,
				out var spamPrevention,
				out var errorReason))
			{
				await MessageUtils.SendErrorMessageAsync(Context, errorReason).CAF();
				return;
			}

			Context.GuildSettings.SpamPreventionDictionary[spamType] = spamPrevention;
			var resp = $"Successfully set up the spam prevention for `{spamType.EnumName().ToLower()}`.\n{spamPrevention.ToString()}";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
		[Command(nameof(Enable)), ShortAlias(nameof(Enable))]
		public async Task Enable(SpamType spamType)
		{
			var spamPrev = Context.GuildSettings.SpamPreventionDictionary[spamType];
			if (spamPrev == null)
			{
				var error = new ErrorReason("There must be a spam prevention of that type set up before one can be enabled or disabled.");
				await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
				return;
			}

			spamPrev.Enable();
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, "Successfully enabled the given spam prevention.").CAF();
		}
		[Command(nameof(Disable)), ShortAlias(nameof(Disable))]
		public async Task Disable(SpamType spamType)
		{
			var spamPrev = Context.GuildSettings.SpamPreventionDictionary[spamType];
			if (spamPrev == null)
			{
				var error = new ErrorReason("There must be a spam prevention of that type set up before one can be enabled or disabled.");
				await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
				return;
			}

			spamPrev.Disable();
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, "Successfully disabled the given spam prevention.").CAF();
		}
	}

	[Group(nameof(PreventRaid)), TopLevelShortAlias(typeof(PreventRaid))]
	[Summary("Any users who joins from now on will get text muted. " +
		"Once `preventraidspam` is turned off all the users who were muted will be unmuted. " +
		"Inputting a number means the last x amount of people (up to 25) who have joined will be muted. " +
		"`Show` lists all of the available punishments.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	public sealed class PreventRaid : SavingModuleBase
	{
		[Command(nameof(Show)), ShortAlias(nameof(Show))]
		public async Task Show()
		{
			var desc = $"`{String.Join("`, `", Enum.GetNames(typeof(PunishmentType)))}`";
			await MessageUtils.SendEmbedMessageAsync(Context.Channel, new EmbedWrapper("Punishment Types", desc)).CAF();
		}
		[Command(nameof(Create)), ShortAlias(nameof(Create))]
		public async Task Create(RaidType raidType, PunishmentType punishment, uint userCount, uint interval)
		{
			if (!RaidPreventionInfo.TryCreateRaidPreventionInfo(
				raidType, 
				punishment, 
				(int)userCount, 
				(int)interval, 
				out var raidPrevention, 
				out var errorReason))
			{
				await MessageUtils.SendErrorMessageAsync(Context, errorReason).CAF();
				return;
			}

			Context.GuildSettings.RaidPreventionDictionary[raidType] = raidPrevention;
			var resp = $"Successfully set up the raid prevention for `{raidType.EnumName().ToLower()}`.\n{raidPrevention.ToString()}";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
		[Command(nameof(Enable)), ShortAlias(nameof(Enable))]
		public async Task Enable(RaidType raidType)
		{
			var raidPrev = Context.GuildSettings.RaidPreventionDictionary[raidType];
			if (raidPrev == null)
			{
				var error = new ErrorReason("There must be a raid prevention of that type set up before one can be enabled or disabled.");
				await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
				return;
			}

			raidPrev.Enabled = true;
			if (raidType == RaidType.Regular)
			{
				//Mute the newest joining users
				var users = (await Context.Guild.GetUsersAndOrderByJoinAsync().CAF()).Reverse().ToArray();
				for (int i = 0; i < new[] { raidPrev.UserCount, users.Length, 25 }.Min(); ++i)
				{
					await raidPrev.PunishAsync(Context.GuildSettings, users[i]).CAF();
				}
			}

			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, "Successfully enabled the given raid prevention.").CAF();
		}
		[Command(nameof(Disable)), ShortAlias(nameof(Disable))]
		public async Task Disable(RaidType raidType)
		{
			var raidPrev = Context.GuildSettings.RaidPreventionDictionary[raidType];
			if (raidPrev == null)
			{
				var error = new ErrorReason("There must be a raid prevention of that type set up before one can be enabled or disabled.");
				await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
				return;
			}

			raidPrev.Enabled = false;
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, "Successfully disabled the given raid prevention.").CAF();
		}
	}
}
