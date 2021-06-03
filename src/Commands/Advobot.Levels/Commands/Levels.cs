using System.Threading.Tasks;

using Advobot.Attributes;
using Advobot.Attributes.ParameterPreconditions.Numbers;
using Advobot.Levels.Database;
using Advobot.Levels.Utilities;
using Advobot.Localization;
using Advobot.Resources;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

using static Advobot.Levels.Responses.Levels;

namespace Advobot.Levels.Commands
{
	[Category(nameof(Levels))]
	[LocalizedGroup(nameof(Groups.Levels))]
	[LocalizedAlias(nameof(Aliases.Levels))]
	public sealed class Levels : ModuleBase
	{
		[LocalizedGroup(nameof(Groups.Show))]
		[LocalizedAlias(nameof(Aliases.Show))]
		[LocalizedSummary(nameof(Summaries.LevelsShow))]
		[Meta("bebda6ba-6fbf-4278-94e0-408dcdc77d3c", IsEnabled = true)]
		public sealed class Show : LevelModuleBase
		{
			[Command]
			public Task<RuntimeResult> Command()
				=> Command(new SearchArgs(Context.User.Id, Context.Guild.Id));

			[Command]
			public Task<RuntimeResult> Command(IUser user)
				=> Command(new SearchArgs(user.Id, Context.Guild.Id));

			[Command]
			public Task<RuntimeResult> Command(ulong userId)
				=> Command(new SearchArgs(userId, Context.Guild.Id));

			[Command]
			public async Task<RuntimeResult> Command(SearchArgs args)
			{
				var rank = await Db.GetRankAsync(args).CAF();
				var user = await Context.Client.GetUserAsync(rank.UserId).CAF();
				if (rank.Experience == 0)
				{
					return NoXp(args, rank, user);
				}

				var level = Service.CalculateLevel(rank.Experience);
				return Level(args, rank, level, user);
			}
		}

		[LocalizedGroup(nameof(Groups.Top))]
		[LocalizedAlias(nameof(Aliases.Top))]
		[LocalizedSummary(nameof(Summaries.LevelsTop))]
		[Meta("649ec476-4043-48b0-9802-62a9288d007b", IsEnabled = true)]
		public sealed class Top : LevelModuleBase
		{
			public const int PAGE_LENGTH = 15;

			private ulong ChannelId => Context.Channel.Id;
			private ulong GuildId => Context.Guild.Id;

			[Command(nameof(Channel))]
			public Task<RuntimeResult> Channel([Positive] int page = 1)
				=> Command(new SearchArgs(guildId: GuildId, channelId: ChannelId), page);

			[Command(nameof(Global))]
			public Task<RuntimeResult> Global([Positive] int page = 1)
				=> Command(new SearchArgs(), page);

			[Command]
			public Task<RuntimeResult> Guild([Positive] int page = 1)
				=> Command(new SearchArgs(guildId: GuildId), page);

			private async Task<RuntimeResult> Command(SearchArgs args, int page)
			{
				var offset = PAGE_LENGTH * (page - 1);
				var ranks = await Db.GetRanksAsync(args, offset, PAGE_LENGTH).CAF();
				return Responses.Levels.Top(args, ranks, x =>
				{
					var level = Service.CalculateLevel(x.Experience);
					var user = Context.Client.GetUser(x.UserId);
					return (level, user);
				});
			}
		}
	}
}