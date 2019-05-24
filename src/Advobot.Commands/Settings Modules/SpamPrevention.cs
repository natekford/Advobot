using System.Threading.Tasks;
using Advobot.Classes.Attributes;
using Advobot.Classes.Modules;
using Advobot.Classes.Settings;
using Advobot.Enums;
using Advobot.Interfaces;
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
			public Task<RuntimeResult> Create(SpamType spamType, [Remainder] SpamPrev args)
			{
				Settings.SpamPrevention.RemoveAll(x => x.Type == spamType);
				Settings.SpamPrevention.Add(args);
				return Responses.SpamPrevention.CreatedSpamPrevention(spamType);
			}
			[DontSaveAfterExecution]
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Enable(SpamType spamType)
				=> CommandRunner(spamType, true);
			[DontSaveAfterExecution]
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Disable(SpamType spamType)
				=> CommandRunner(spamType, false);

			private async Task<RuntimeResult> CommandRunner(SpamType spamType, bool enable)
			{
				if (!Settings.SpamPrevention.TryGetSingle(x => x.Type == spamType, out var antiSpam))
				{
					return Responses.SpamPrevention.NoSpamPrevention(spamType);
				}

				await (enable ? antiSpam.EnableAsync(Context.Guild) : antiSpam.DisableAsync(Context.Guild)).CAF();
				return Responses.SpamPrevention.ToggledSpamPrevention(spamType, enable);
			}
		}

		[Group(nameof(PreventRaid)), ModuleInitialismAlias(typeof(PreventRaid))]
		[Summary("Any users who joins from now on will get text muted. " +
			"Once `" + nameof(PreventRaid) + "` is turned off all the users who were muted will be unmuted. " +
			"Inputting a number means the last x amount of people (up to 25) who have joined will be muted.")]
		[RequireUserPermission(GuildPermission.Administrator)]
		[EnabledByDefault(false)]
		//[SaveGuildSettings]
		public sealed class PreventRaid : AdvobotSettingsModuleBase<IGuildSettings>
		{
			protected override IGuildSettings Settings => Context.GuildSettings;

			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Create(RaidType raidType, [Remainder] RaidPrev args)
			{
				Settings.RaidPrevention.RemoveAll(x => x.Type == raidType);
				Settings.RaidPrevention.Add(args);
				return Responses.SpamPrevention.CreatedRaidPrevention(raidType);
			}
			[DontSaveAfterExecution]
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Enable(RaidType raidType)
				=> CommandRunner(raidType, true);
			[DontSaveAfterExecution]
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Disable(RaidType raidType)
				=> CommandRunner(raidType, false);

			private async Task<RuntimeResult> CommandRunner(RaidType raidType, bool enable)
			{
				if (!Settings.RaidPrevention.TryGetSingle(x => x.Type == raidType, out var antiRaid))
				{
					return Responses.SpamPrevention.NoRaidPrevention(raidType);
				}

				await (enable ? antiRaid.EnableAsync(Context.Guild) : antiRaid.DisableAsync(Context.Guild)).CAF();
				return Responses.SpamPrevention.ToggledRaidPrevention(raidType, enable);
			}
		}
	}
}
