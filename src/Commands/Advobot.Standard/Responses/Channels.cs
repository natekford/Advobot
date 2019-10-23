using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using Advobot.Classes;
using Advobot.Modules;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;

using static Advobot.Standard.Resources.Responses;

namespace Advobot.Standard.Responses
{
	public sealed class Channels : AdvobotResult
	{
		private Channels() : base(null, "")
		{
		}

		public static AdvobotResult ClearedOverwrites(IGuildChannel channel, int count)
		{
			return Success(ChannelsClearedOverwrites.Format(
				count.ToString().WithBlock(),
				channel.Format().WithBlock()
			));
		}

		public static AdvobotResult CopiedOverwrites(
			IGuildChannel input,
			IGuildChannel output,
			ISnowflakeEntity? obj,
			IReadOnlyCollection<Overwrite> overwrites)
		{
			if (overwrites.Count == 0)
			{
				return Success(ChannelsNoCopyableOverwrite);
			}

			return Success(ChannelsCopiedOverwrite.Format(
				(obj?.Format() ?? ChannelsVariableAllOverwrites).WithBlock(),
				input.Format().WithBlock(),
				output.Format().WithBlock()
			));
		}

		public static AdvobotResult CreatededRoleRestrictedChannel(
			IVoiceChannel channel,
			IRole role)
		{
			return Success(ChannelsCreatededRoleRestrictedChannel.Format(
				channel.Format().WithBlock(),
				role.Format().WithBlock()
			));
		}

		public static AdvobotResult Display(
			IEnumerable<IGuildChannel> channels,
			[CallerMemberName] string caller = "")
		{
			var title = ChannelsTitleChannelPositions.Format(
				caller.WithNoMarkdown()
			);
			var description = channels
				.OrderBy(x => x.Position)
				.Join(x => $"{x.Position.ToString("00")}. {x.Name}", "\n")
				.WithBigBlock()
				.Value;
			return Success(new EmbedWrapper
			{
				Title = title,
				Description = description,
			});
		}

		public static AdvobotResult DisplayOverwrite(
			IGuildChannel channel,
			ISnowflakeEntity obj,
			IDictionary<ChannelPermission, PermValue> values)
		{
			var title = ChannelsTitleSingleOverwrite.Format(
				obj.Format().WithNoMarkdown(),
				channel.Format().WithNoMarkdown()
			);
			var description = values
				.FormatPermissionValues(x => x.ToString(), out var padLen)
				.Join(x => $"{x.Key.PadRight(padLen)} {x.Value}", Environment.NewLine)
				.WithBigBlock()
				.Value;
			return Success(new EmbedWrapper
			{
				Title = title,
				Description = description,
			});
		}

		public static AdvobotResult DisplayOverwrites(
			IGuildChannel channel,
			IEnumerable<string> roleNames,
			IEnumerable<string> userNames)
		{
			var title = ChannelsTitleAllOverwrites.Format(
				channel.Format().WithNoMarkdown()
			);
			var embed = new EmbedWrapper
			{
				Title = title,
			};

			var rolesValue = roleNames.Join().WithBigBlock().Value;
			embed.TryAddField(ChannelsTitleAllOverwritesRoles, rolesValue, false, out _);
			var usersValue = userNames.Join().WithBigBlock().Value;
			embed.TryAddField(ChannelsTitleAllOverwritesUsers, usersValue, false, out _);

			return Success(embed);
		}

		public static AdvobotResult MismatchType(
			IGuildChannel input,
			IGuildChannel output)
		{
			return Failure(ChannelsFailedPermissionCopy.Format(
				input.Format().WithBlock(),
				output.Format().WithBlock()
			));
		}

		public static AdvobotResult ModifiedBitrate(IVoiceChannel channel, int bitrate)
		{
			return Success(ChannelsModifiedBitrate.Format(
				channel.Format().WithBlock(),
				bitrate.ToString().WithBlock()
			));
		}

		public static AdvobotResult ModifiedLimit(IVoiceChannel channel, int limit)
		{
			return Success(ChannelsModifiedLimit.Format(
				channel.Format().WithBlock(),
				limit.ToString().WithBlock()
			));
		}

		public static AdvobotResult ModifiedNsfw(ITextChannel channel, bool nsfw)
		{
			return Success(ChannelsModifiedNsfw.Format(
				channel.Format().WithBlock(),
				nsfw.ToString().WithBlock()
			));
		}

		public static AdvobotResult ModifiedOverwrite(
			IGuildChannel channel,
			ISnowflakeEntity obj,
			ChannelPermission permissions,
			PermValue action)
		{
			var format = action switch
			{
				PermValue.Allow => ChannelsModifiedOverwriteAllow,
				PermValue.Inherit => ChannelsModifiedOverwriteInherit,
				PermValue.Deny => ChannelsModifiedOverwriteDeny,
				_ => throw new ArgumentOutOfRangeException(nameof(action)),
			};

			return Success(format.Format(
				EnumUtils.GetFlagNames(permissions).Join().WithBlock(),
				obj.Format().WithBlock(),
				channel.Format().WithBlock()
			));
		}

		public static AdvobotResult ModifiedTopic(ITextChannel channel, string topic)
		{
			return Success(ChannelsModifiedTopic.Format(
				channel.Format().WithBlock(),
				topic.WithBlock()
			));
		}

		public static AdvobotResult Moved(IGuildChannel channel, int position)
		{
			return Success(ChannelsMoved.Format(
				channel.Format().WithBlock(),
				position.ToString().WithBlock()
			));
		}

		public static AdvobotResult NoOverwriteFound(
			IGuildChannel channel,
			ISnowflakeEntity obj)
		{
			return Success(ChannelsNoOverwrite.Format(
				obj.Format().WithBlock(),
				channel.Format().WithBlock()
			));
		}

		public static AdvobotResult RemovedTopic(ITextChannel channel)
		{
			return Success(ChannelsRemovedTopic.Format(
				channel.Format().WithBlock()
			));
		}
	}
}