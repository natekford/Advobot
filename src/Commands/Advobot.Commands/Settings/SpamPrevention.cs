using System.Threading.Tasks;
using Advobot.Attributes;
using Advobot.Attributes.Preconditions.Permissions;
using Advobot.Commands.Localization;
using Advobot.Commands.Resources;
using Advobot.Modules;
using Advobot.Services.GuildSettings;
using Advobot.Services.GuildSettings.Settings;
using AdvorangesUtils;
using Discord.Commands;

namespace Advobot.Commands.Settings
{
	public sealed class SpamPrevention : ModuleBase
	{
		[Group(nameof(PreventSpam)), ModuleInitialismAlias(typeof(PreventSpam))]
		[LocalizedSummary(nameof(Summaries.PreventSpam))]
		[CommandMeta("901e3443-0ed9-41cd-9d29-1dc890f3c329")]
		[RequireGuildPermissions]
		public sealed class PreventSpam : SettingsModule<IGuildSettings>
		{
			protected override IGuildSettings Settings => Context.Settings;

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
		[LocalizedSummary(nameof(Summaries.PreventRaid))]
		[CommandMeta("9e11556d-f61b-4921-936b-ecf1b6fa0582")]
		[RequireGuildPermissions]
		public sealed class PreventRaid : SettingsModule<IGuildSettings>
		{
			protected override IGuildSettings Settings => Context.Settings;

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
