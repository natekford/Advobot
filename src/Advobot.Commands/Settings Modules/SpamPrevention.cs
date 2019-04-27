using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes.Attributes;
using Advobot.Classes.Modules;
using Advobot.Classes.Settings;
using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;

namespace Advobot.Commands
{
	public sealed class SpamPrevention : ModuleBase
	{
		[Group(nameof(PreventSpam)), ModuleInitialismAlias(typeof(PreventSpam))]
		[Summary("Spam prevention allows for some protection against mention spammers. " +
			"Messages is the amount of messages a user has to send with the given amount of mentions before being considered as potential spam. " +
			"Votes is the amount of users that have to agree with the potential punishment. " +
			"The spam users are reset every hour.")]
		[RequireUserPermission(GuildPermission.Administrator)]
		[EnabledByDefault(false)]
		//[SaveGuildSettings]
		public sealed class PreventSpam : AdvobotSettingsModuleBase<IGuildSettings>
		{
			protected override IGuildSettings Settings => Context.GuildSettings;

			[ImplicitCommand, ImplicitAlias]
			public Task Create(SpamType spamType, [Remainder] SpamPrev args)
			{
				Settings[spamType] = args;
				return ReplyTimedAsync($"Successfully set up the spam prevention for `{args.Type}`.\n{args}");
			}
			[DontSaveAfterExecution]
			[ImplicitCommand, ImplicitAlias]
			public Task Enable(SpamType spamType)
				=> CommandRunner(spamType, true);
			[DontSaveAfterExecution]
			[ImplicitCommand, ImplicitAlias]
			public Task Disable(SpamType spamType)
				=> CommandRunner(spamType, false);

			private Task CommandRunner(SpamType spamType, bool enable)
			{
				if (!(Settings[spamType] is SpamPrev antiSpam))
				{
					return ReplyErrorAsync($"There must be a `{spamType}` spam prevention before one can be enabled or disabled.");
				}

				antiSpam.Enabled = enable;
				return ReplyTimedAsync($"Successfully {(enable ? "enabled" : "disabled")} the `{spamType}` spam prevention.");
			}
		}

		[Group(nameof(PreventRaid)), ModuleInitialismAlias(typeof(PreventRaid))]
		[Summary("Any users who joins from now on will get text muted. " +
			"Once `preventraid` is turned off all the users who were muted will be unmuted. " +
			"Inputting a number means the last x amount of people (up to 25) who have joined will be muted.")]
		[RequireUserPermission(GuildPermission.Administrator)]
		[EnabledByDefault(false)]
		//[SaveGuildSettings]
		public sealed class PreventRaid : AdvobotSettingsModuleBase<IGuildSettings>
		{
			protected override IGuildSettings Settings => Context.GuildSettings;

			[ImplicitCommand, ImplicitAlias]
			public Task Create(RaidType raidType, [Remainder] RaidPrev args)
			{
				Settings[raidType] = args;
				return ReplyTimedAsync($"Successfully set up the raid prevention for `{args.Type}`.\n{args}");
			}
			[DontSaveAfterExecution]
			[ImplicitCommand, ImplicitAlias]
			public Task Enable(RaidType raidType)
				=> CommandRunner(raidType, true);
			[DontSaveAfterExecution]
			[ImplicitCommand, ImplicitAlias]
			public Task Disable(RaidType raidType)
				=> CommandRunner(raidType, false);

			private async Task CommandRunner(RaidType raidType, bool enable)
			{
				if (!(Settings[raidType] is RaidPrev antiRaid))
				{
					await ReplyErrorAsync($"There must be a `{raidType}` raid prevention before one can be enabled or disabled.").CAF();
					return;
				}

				antiRaid.Enabled = enable;
				if (enable && raidType == RaidType.Regular)
				{
					//Mute the newest joining users
					var users = Context.Guild.GetUsersByJoinDate().Reverse().ToArray();
					for (var i = 0; i < new[] { antiRaid.UserCount, users.Length, 25 }.Min(); ++i)
					{
						await antiRaid.PunishAsync(Settings, users[i]).CAF();
					}
				}

				await ReplyTimedAsync($"Successfully {(enable ? "enabled" : "disabled")} the `{raidType}` raid prevention.").CAF();
			}
		}
	}
}
