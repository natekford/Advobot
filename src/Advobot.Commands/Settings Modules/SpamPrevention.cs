using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.Modules;
using Advobot.Classes.Settings;
using Advobot.Enums;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;

namespace Advobot.Commands
{
	[Group]
	public sealed class SpamPrevention : ModuleBase
	{
		[Group(nameof(PreventSpam)), TopLevelShortAlias(typeof(PreventSpam))]
		[Summary("Spam prevention allows for some protection against mention spammers. " +
			"Messages is the amount of messages a user has to send with the given amount of mentions before being considered as potential spam. " +
			"Votes is the amount of users that have to agree with the potential punishment. " +
			"The spam users are reset every hour.")]
		[RequireUserPermission(GuildPermission.Administrator)]
		[EnabledByDefault(false)]
		//[SaveGuildSettings]
		public sealed class PreventSpam : AdvobotModuleBase
		{
			[Command(nameof(Create)), ShortAlias(nameof(Create))]
			public async Task Create(SpamPrev args)
			{
				Context.GuildSettings[args.Type] = args;
				await ReplyTimedAsync($"Successfully set up the spam prevention for `{args.Type}`.\n{args}").CAF();
			}
			[Command(nameof(Enable)), ShortAlias(nameof(Enable))]
			public async Task Enable(SpamType spamType)
				=> await CommandRunner(spamType, true).CAF();
			[Command(nameof(Disable)), ShortAlias(nameof(Disable))]
			public async Task Disable(SpamType spamType)
				=> await CommandRunner(spamType, false).CAF();

			private async Task CommandRunner(SpamType spamType, bool enable)
			{
				if (!(Context.GuildSettings[spamType] is SpamPrev antiSpam))
				{
					await ReplyErrorAsync($"There must be a `{spamType}` spam prevention before one can be enabled or disabled.").CAF();
					return;
				}

				antiSpam.Enabled = enable;
				await ReplyTimedAsync($"Successfully {(enable ? "enabled" : "disabled")} the `{spamType}` spam prevention.").CAF();
			}
		}

		[Group(nameof(PreventRaid)), TopLevelShortAlias(typeof(PreventRaid))]
		[Summary("Any users who joins from now on will get text muted. " +
			"Once `preventraid` is turned off all the users who were muted will be unmuted. " +
			"Inputting a number means the last x amount of people (up to 25) who have joined will be muted.")]
		[RequireUserPermission(GuildPermission.Administrator)]
		[EnabledByDefault(false)]
		//[SaveGuildSettings]
		public sealed class PreventRaid : AdvobotModuleBase
		{
			[Command(nameof(Create)), ShortAlias(nameof(Create))]
			public async Task Create([Remainder] RaidPrev args)
			{
				Context.GuildSettings[args.Type] = args;
				await ReplyTimedAsync($"Successfully set up the raid prevention for `{args.Type}`.\n{args}").CAF();
			}
			[Command(nameof(Enable)), ShortAlias(nameof(Enable))]
			public async Task Enable(RaidType raidType)
				=> await CommandRunner(raidType, true).CAF();
			[Command(nameof(Disable)), ShortAlias(nameof(Disable))]
			public async Task Disable(RaidType raidType)
				=> await CommandRunner(raidType, false).CAF();

			private async Task CommandRunner(RaidType raidType, bool enable)
			{
				if (!(Context.GuildSettings[raidType] is RaidPrev antiRaid))
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
						await antiRaid.PunishAsync(Context.GuildSettings, users[i]).CAF();
					}
				}

				await ReplyTimedAsync($"Successfully {(enable ? "enabled" : "disabled")} the `{raidType}` raid prevention.").CAF();
			}
		}
	}
}
