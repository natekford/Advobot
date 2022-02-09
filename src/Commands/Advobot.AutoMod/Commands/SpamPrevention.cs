using Advobot.Attributes;
using Advobot.AutoMod.Models;
using Advobot.Localization;
using Advobot.Preconditions.Permissions;
using Advobot.Resources;

using AdvorangesUtils;

using Discord.Commands;

using static Advobot.AutoMod.Responses.SpamPrevention;

namespace Advobot.AutoMod.Commands;

[Category(nameof(SpamPrevention))]
public sealed class SpamPrevention : ModuleBase
{
	[LocalizedGroup(nameof(Groups.PreventRaid))]
	[LocalizedAlias(nameof(Aliases.PreventRaid))]
	[LocalizedSummary(nameof(Summaries.PreventRaid))]
	[Meta("9e11556d-f61b-4921-936b-ecf1b6fa0582")]
	[RequireGuildPermissions]
	public sealed class PreventRaid : AutoModModuleBase
	{
		[LocalizedCommand(nameof(Groups.Create))]
		[LocalizedAlias(nameof(Aliases.Create))]
		public async Task<RuntimeResult> Create(RaidType raidType, TimedPunishmentArgs args)
		{
			var current = await Db.GetRaidPreventionAsync(Context.Guild.Id, raidType).CAF();
			var @new = args.Create<RaidPrevention>(current) with
			{
				GuildId = Context.Guild.Id,
				RaidType = raidType,
			};

			await Db.UpsertRaidPreventionAsync(@new).CAF();
			return CreatedPrevention(raidType);
		}

		[LocalizedCommand(nameof(Groups.Disable))]
		[LocalizedAlias(nameof(Aliases.Disable))]
		public Task<RuntimeResult> Disable(RaidType raidType)
			=> CommandRunner(raidType, false);

		[LocalizedCommand(nameof(Groups.Enable))]
		[LocalizedAlias(nameof(Aliases.Enable))]
		public Task<RuntimeResult> Enable(RaidType raidType)
			=> CommandRunner(raidType, true);

		private async Task<RuntimeResult> CommandRunner(RaidType raidType, bool enable)
		{
			var current = await Db.GetRaidPreventionAsync(Context.Guild.Id, raidType).CAF();
			if (current is null)
			{
				return NoPrevention(raidType);
			}
			else if (current.Enabled == enable)
			{
				return AlreadyToggledPrevention(raidType, enable);
			}

			await Db.UpsertRaidPreventionAsync(current with
			{
				Enabled = !current.Enabled,
			}).CAF();
			return ToggledPrevention(raidType, enable);
		}
	}

	[LocalizedGroup(nameof(Groups.PreventSpam))]
	[LocalizedAlias(nameof(Aliases.PreventSpam))]
	[LocalizedSummary(nameof(Summaries.PreventSpam))]
	[Meta("901e3443-0ed9-41cd-9d29-1dc890f3c329")]
	[RequireGuildPermissions]
	public sealed class PreventSpam : AutoModModuleBase
	{
		[LocalizedCommand(nameof(Groups.Create))]
		[LocalizedAlias(nameof(Aliases.Create))]
		public async Task<RuntimeResult> Create(SpamType spamType, TimedPunishmentArgs args)
		{
			var current = await Db.GetSpamPreventionAsync(Context.Guild.Id, spamType).CAF();
			var @new = args.Create<Models.SpamPrevention>(current) with
			{
				GuildId = Context.Guild.Id,
				SpamType = spamType,
			};

			await Db.UpsertSpamPreventionAsync(@new).CAF();
			return CreatedPrevention(spamType);
		}

		[LocalizedCommand(nameof(Groups.Disable))]
		[LocalizedAlias(nameof(Aliases.Disable))]
		public Task<RuntimeResult> Disable(SpamType spamType)
			=> CommandRunner(spamType, false);

		[LocalizedCommand(nameof(Groups.Enable))]
		[LocalizedAlias(nameof(Aliases.Enable))]
		public Task<RuntimeResult> Enable(SpamType spamType)
			=> CommandRunner(spamType, true);

		private async Task<RuntimeResult> CommandRunner(SpamType spamType, bool enable)
		{
			var current = await Db.GetSpamPreventionAsync(Context.Guild.Id, spamType).CAF();
			if (current is null)
			{
				return NoPrevention(spamType);
			}
			else if (current.Enabled == enable)
			{
				return AlreadyToggledPrevention(spamType, enable);
			}

			await Db.UpsertSpamPreventionAsync(current with
			{
				Enabled = !current.Enabled,
			}).CAF();
			return ToggledPrevention(spamType, enable);
		}
	}
}