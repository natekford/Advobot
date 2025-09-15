using Advobot.Attributes;
using Advobot.Levels.Database;
using Advobot.Levels.Database.Models;
using Advobot.Modules;
using Advobot.ParameterPreconditions.Numbers;
using Advobot.Resources;

using Discord;

using YACCS.Commands.Attributes;
using YACCS.Localization;

using static Advobot.Levels.Responses.Levels;

namespace Advobot.Levels.Commands;

[LocalizedCategory(nameof(Levels))]
[LocalizedCommand(nameof(Groups.Levels), nameof(Aliases.Levels))]
public sealed class Levels : AdvobotModuleBase
{
	[LocalizedCommand(nameof(Groups.Show), nameof(Aliases.Show))]
	[LocalizedSummary(nameof(Summaries.LevelsShow))]
	[Id("bebda6ba-6fbf-4278-94e0-408dcdc77d3c")]
	[Meta(IsEnabled = true)]
	public sealed class Show : LevelModuleBase
	{
		[LocalizedCommand]
		public Task<AdvobotResult> Command()
			=> Command(new SearchArgs(Context.User.Id, Context.Guild.Id));

		[LocalizedCommand]
		public Task<AdvobotResult> Command(IGuildUser user)
			=> Command(new SearchArgs(user.Id, Context.Guild.Id));

		[LocalizedCommand]
		public Task<AdvobotResult> Command(ulong userId)
			=> Command(new SearchArgs(userId, Context.Guild.Id));

		[LocalizedCommand]
		public async Task<AdvobotResult> Command(SearchArgs args)
		{
			var rank = await Db.GetRankAsync(args).ConfigureAwait(false);
			var user = await GetUserAsync(rank.UserId).ConfigureAwait(false);
			if (rank.Experience == 0)
			{
				return NoXp(args, rank, user);
			}

			var level = Service.CalculateLevel(rank.Experience);
			return Level(args, rank, level, user);
		}
	}

	[LocalizedCommand(nameof(Groups.Top), nameof(Aliases.Top))]
	[LocalizedSummary(nameof(Summaries.LevelsTop))]
	[Id("649ec476-4043-48b0-9802-62a9288d007b")]
	[Meta(IsEnabled = true)]
	public sealed class Top : LevelModuleBase
	{
		public const int PAGE_LENGTH = 15;

		private ulong ChannelId => Context.Channel.Id;
		private ulong GuildId => Context.Guild.Id;

		[LocalizedCommand(nameof(Groups.Channel), nameof(Aliases.Channel))]
		public Task<AdvobotResult> Channel([Positive] int page = 1)
			=> Command(new SearchArgs(guildId: GuildId, channelId: ChannelId), page);

		[LocalizedCommand(nameof(Groups.Global), nameof(Aliases.Global))]
		public Task<AdvobotResult> Global([Positive] int page = 1)
			=> Command(new SearchArgs(), page);

		[LocalizedCommand]
		public Task<AdvobotResult> Guild([Positive] int page = 1)
			=> Command(new SearchArgs(guildId: GuildId), page);

		private async Task<AdvobotResult> Command(SearchArgs args, int page)
		{
			var offset = PAGE_LENGTH * (page - 1);
			var ranks = await Db.GetRanksAsync(args, offset, PAGE_LENGTH).ConfigureAwait(false);
			var rankDescriptions = new List<(IRank, int, IUser?)>(ranks.Count);
			foreach (var rank in ranks)
			{
				var level = Service.CalculateLevel(rank.Experience);
				var user = await GetUserAsync(rank.UserId).ConfigureAwait(false);
				rankDescriptions.Add((rank, level, user));
			}

			return Responses.Levels.Top(args, rankDescriptions);
		}
	}
}