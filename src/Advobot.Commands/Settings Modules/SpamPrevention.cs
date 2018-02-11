﻿using Advobot.Core.Classes;
using Advobot.Core.Classes.Attributes;
using Advobot.Core.Classes.Settings;
using Advobot.Core.Enums;
using Advobot.Core.Utilities;
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
	public sealed class PreventSpam : GuildSettingsSavingModuleBase
	{
		[Command(nameof(Show)), ShortAlias(nameof(Show))]
		public async Task Show()
		{
			var embed = new EmbedWrapper
			{
				Title = "Punishment Types",
				Description = $"`{String.Join("`, `", Enum.GetNames(typeof(PunishmentType)))}`"
			};
			await MessageUtils.SendEmbedMessageAsync(Context.Channel, embed).CAF();
		}
		[Command(nameof(Create)), ShortAlias(nameof(Create))]
		public async Task Create(SpamType spam, PunishmentType punishment, uint messageCount, uint votes, uint timeInterval, uint spamAmount)
		{
			if (!SpamPreventionInfo.TryCreate(spam, punishment, (int)messageCount, (int)votes, (int)timeInterval, (int)spamAmount,
				out var prev, out var error))
			{
				await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
				return;
			}

			Context.GuildSettings.SpamPreventionDictionary[spam] = prev;
			var resp = $"Successfully set up the spam prevention for `{spam.ToString().ToLower()}`.\n{prev}";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
		[Command(nameof(Enable)), ShortAlias(nameof(Enable))]
		public async Task Enable(SpamType spamType)
		{
			var spamPrev = Context.GuildSettings.SpamPreventionDictionary[spamType];
			if (spamPrev == null)
			{
				var error = new Error("There must be a spam prevention of that type set up before one can be enabled or disabled.");
				await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
				return;
			}

			spamPrev.Enabled = true;
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, "Successfully enabled the given spam prevention.").CAF();
		}
		[Command(nameof(Disable)), ShortAlias(nameof(Disable))]
		public async Task Disable(SpamType spamType)
		{
			var spamPrev = Context.GuildSettings.SpamPreventionDictionary[spamType];
			if (spamPrev == null)
			{
				var error = new Error("There must be a spam prevention of that type set up before one can be enabled or disabled.");
				await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
				return;
			}

			spamPrev.Enabled = false;
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
	public sealed class PreventRaid : GuildSettingsSavingModuleBase
	{
		[Command(nameof(Show)), ShortAlias(nameof(Show))]
		public async Task Show()
		{
			var embed = new EmbedWrapper
			{
				Title = "Punishment Types",
				Description = $"`{String.Join("`, `", Enum.GetNames(typeof(PunishmentType)))}`"
			};
			await MessageUtils.SendEmbedMessageAsync(Context.Channel, embed).CAF();
		}
		[Command(nameof(Create)), ShortAlias(nameof(Create))]
		public async Task Create(RaidType raidType, PunishmentType punishment, uint userCount, uint interval)
		{
			if (!RaidPreventionInfo.TryCreate(
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
			var resp = $"Successfully set up the raid prevention for `{raidType.ToString().ToLower()}`.\n{raidPrevention}";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
		[Command(nameof(Enable)), ShortAlias(nameof(Enable))]
		public async Task Enable(RaidType raidType)
		{
			var raidPrev = Context.GuildSettings.RaidPreventionDictionary[raidType];
			if (raidPrev == null)
			{
				var error = new Error("There must be a raid prevention of that type set up before one can be enabled or disabled.");
				await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
				return;
			}

			raidPrev.Enabled = true;
			if (raidType == RaidType.Regular)
			{
				//Mute the newest joining users
				var users = Context.Guild.GetUsersByJoinDate().Reverse().ToArray();
				for (var i = 0; i < new[] { raidPrev.UserCount, users.Length, 25 }.Min(); ++i)
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
				var error = new Error("There must be a raid prevention of that type set up before one can be enabled or disabled.");
				await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
				return;
			}

			raidPrev.Enabled = false;
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, "Successfully disabled the given raid prevention.").CAF();
		}
	}
}