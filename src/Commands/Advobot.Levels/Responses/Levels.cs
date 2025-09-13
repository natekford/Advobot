using Advobot.Embeds;
using Advobot.Levels.Database;
using Advobot.Levels.Database.Models;
using Advobot.Modules;
using Advobot.Utilities;

using Discord;

using static Advobot.Resources.Responses;

namespace Advobot.Levels.Responses;

public sealed class Levels : AdvobotResult
{
	public static AdvobotResult Level(SearchArgs args, IRank rank, int level, IUser? user)
	{
		var title = LevelsLevelTitle.Format(
			GetSearchType(args).WithTitleCase(),
			FormatUser(rank, user).WithNoMarkdown()
		);
		var description = LevelsLevelDescription.Format(
			rank.Position.ToString().WithBlock(),
			rank.TotalRankCount.ToString().WithBlock(),
			rank.Experience.ToString().WithBlock(),
			level.ToString().WithBlock()
		);
		return Success(new EmbedWrapper
		{
			Title = title,
			Description = description,
			Author = user?.CreateAuthor() ?? new(),
			Footer = new()
			{
				Text = LevelsLevelFooter,
			},
		});
	}

	public static AdvobotResult NoXp(SearchArgs args, IRank rank, IUser? user)
	{
		return Success(LevelsNoXp.Format(
			FormatUser(rank, user).WithBlock(),
			GetSearchType(args).WithNoMarkdown()
		));
	}

	public static AdvobotResult Top(
		SearchArgs args,
		IEnumerable<(IRank Rank, int Level, IUser? User)> ranks)
	{
		var title = LevelsTopTitle.Format(
			GetSearchType(args).WithTitleCase()
		);
		var description = ranks.Select(x =>
		{
			var (rank, level, user) = x;
			return LevelsTopDescription.Format(
				(rank.Position + 1).ToString().WithBlock(),
				FormatUser(rank, user).WithBlock(),
				rank.Experience.ToString().WithNoMarkdown(),
				level.ToString().WithNoMarkdown()
			);
		}).Join(Environment.NewLine);
		return Success(new EmbedWrapper
		{
			Title = title,
			Description = description,
			Footer = new()
			{
				Text = LevelsTopFooter,
			},
		});
	}

	private static string FormatUser(IRank rank, IUser? user)
		=> user?.Format() ?? rank.UserId.ToString();

	private static string GetSearchType(SearchArgs args)
	{
		if (args.ChannelId != null)
		{
			return LevelsVariableChannel;
		}
		else if (args.GuildId != null)
		{
			return LevelsVariableGuild;
		}
		return LevelsVariableGlobal;
	}
}