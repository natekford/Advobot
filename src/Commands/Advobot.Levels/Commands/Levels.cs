using Advobot.Attributes;
using Advobot.Levels.Database;
using Advobot.Levels.Database.Models;
using Advobot.Modules;
using Advobot.ParameterPreconditions.Numbers;
using Advobot.Resources;

using Discord;

using YACCS.Commands.Attributes;
using YACCS.Localization;

namespace Advobot.Levels.Commands;

[LocalizedCategory(nameof(Names.LevelsCategory))]
[LocalizedCommand(nameof(Names.Levels), nameof(Names.LevelsAlias))]
public sealed class Levels : AdvobotModuleBase
{
	[LocalizedCommand(nameof(Names.Show), nameof(Names.ShowAlias))]
	[LocalizedSummary(nameof(Summaries.LevelsShowSummary))]
	[Meta("bebda6ba-6fbf-4278-94e0-408dcdc77d3c", IsEnabled = true)]
	public sealed class Show : LevelModuleBase
	{
		[Command]
		public async Task<AdvobotResult> ShowAsync(SearchArgs args)
		{
			var rank = await Db.GetRankAsync(args).ConfigureAwait(false);
			var user = await GetUserAsync(rank.UserId).ConfigureAwait(false);
			if (rank.Experience == 0)
			{
				return Responses.Levels.NoXp(args, rank, user);
			}

			var level = Service.CalculateLevel(rank.Experience);
			return Responses.Levels.Level(args, rank, level, user);
		}

		[Command]
		public Task<AdvobotResult> User(ulong userId)
			=> ShowAsync(new(userId, Context.Guild.Id));

		[Command]
		public Task<AdvobotResult> User(IGuildUser? user = null)
			=> ShowAsync(new((user ?? Context.User).Id, Context.Guild.Id));
	}

	[LocalizedCommand(nameof(Names.Top), nameof(Names.TopAlias))]
	[LocalizedSummary(nameof(Summaries.LevelsTopSummary))]
	[Meta("649ec476-4043-48b0-9802-62a9288d007b", IsEnabled = true)]
	public sealed class Top : LevelModuleBase
	{
		public const int PAGE_LENGTH = 15;

		[LocalizedCommand(nameof(Names.Channel), nameof(Names.ChannelAlias))]
		public Task<AdvobotResult> Channel([Positive] int page = 1)
			=> ShowAsync(new(guildId: Context.Guild.Id, channelId: Context.Channel.Id), page);

		[LocalizedCommand(nameof(Names.Global), nameof(Names.GlobalAlias))]
		public Task<AdvobotResult> Global([Positive] int page = 1)
			=> ShowAsync(new(), page);

		[Command]
		public Task<AdvobotResult> Guild([Positive] int page = 1)
			=> ShowAsync(new(guildId: Context.Guild.Id), page);

		private async Task<AdvobotResult> ShowAsync(SearchArgs args, int page)
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