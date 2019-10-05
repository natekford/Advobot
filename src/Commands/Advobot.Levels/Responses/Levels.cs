using System;
using System.Collections.Generic;
using System.Linq;

using Advobot.Classes;
using Advobot.Levels.Database;
using Advobot.Levels.Metadata;
using Advobot.Modules;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;

using static Advobot.Levels.Resources.Responses;

namespace Advobot.Levels.Responses
{
	public sealed class Levels : AdvobotResult
	{
		private Levels() : base(null, "")
		{
		}

		public static AdvobotResult Level(ISearchArgs args, IRank rank, int level, IUser? user)
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
				ThumbnailUrl = user?.GetAvatarUrl(),
				Author = user?.CreateAuthor() ?? new EmbedAuthorBuilder(),
				Footer = new EmbedFooterBuilder { Text = LevelsLevelFooter, },
			});
		}

		public static AdvobotResult NoXp(ISearchArgs args, IRank rank, IUser? user)
		{
			return Success(LevelsNoXp.Format(
				FormatUser(rank, user).WithBlock(),
				GetSearchType(args).WithNoMarkdown()
			));
		}

		public static AdvobotResult Top(
			ISearchArgs args,
			IReadOnlyList<IRank> ranks,
			Func<IRank, (int Level, IUser? User)> getInfo)
		{
			var title = LevelsTopTitle.Format(
				GetSearchType(args).WithTitleCase()
			);
			var description = ranks.Join(x =>
			{
				var (level, user) = getInfo(x);
				return LevelsTopDescription.Format(
					(x.Position + 1).ToString().WithBlock(),
					FormatUser(x, user).WithBlock(),
					x.Experience.ToString().WithBlock(),
					level.ToString().WithBlock()
				);
			}, Environment.NewLine);
			return Success(new EmbedWrapper
			{
				Title = title,
				Description = description,
				Footer = new EmbedFooterBuilder { Text = LevelsTopFooter, },
			});
		}

		private static string FormatUser(IRank rank, IUser? user)
			=> user?.Format() ?? rank.UserId.ToString();

		private static string GetSearchType(ISearchArgs args)
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
}