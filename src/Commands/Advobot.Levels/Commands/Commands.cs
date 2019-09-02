using System.Threading.Tasks;

using Advobot.Attributes;
using Advobot.Classes;
using Advobot.Levels.Database;
using Advobot.Modules;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

namespace Advobot.Levels.Commands
{
	[Category(nameof(Levels))]
	public sealed class Levels : ModuleBase
	{
		[Group(nameof(Level)), ModuleInitialismAlias(typeof(Level))]
		[Summary("temp")]
		[Meta("bebda6ba-6fbf-4278-94e0-408dcdc77d3c", IsEnabled = true)]
		public sealed class Level : LevelModuleBase
		{
			[Command]
			public Task<RuntimeResult> Command()
				=> Command(new SearchArgs(Context.User.Id, Context.Guild.Id));

			[Command]
			public Task<RuntimeResult> Command(IGuildUser user)
				=> Command(new SearchArgs(user.Id, user.Guild.Id));

			[Command]
			public async Task<RuntimeResult> Command(SearchArgs args)
			{
				var rank = await Service.GetRankAsync(args).CAF();
				var experience = await Service.GetXpAsync(args).CAF();
				var level = Service.CalculateLevel(experience);

				var user = Context.Client.GetUser(args.UserId);
				var name = user.Format() ?? args.UserId.ToString();
				var description =
					$"Rank: {rank.Position} out of {rank.Total}\n" +
					$"XP: {experience}\n" +
					$"Level: {level}";
				var titlePrefix = args.GuildId == null ? "Global" : "Guild";
				return AdvobotResult.Success(new EmbedWrapper
				{
					Title = $"{titlePrefix} xp information for {name}",
					Description = description,
					ThumbnailUrl = user?.GetAvatarUrl(),
					Author = user?.CreateAuthor() ?? new EmbedAuthorBuilder(),
					Footer = new EmbedFooterBuilder { Text = "Xp Information", },
				});
			}
		}
	}
}