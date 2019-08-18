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
	public sealed class Channels : CommandResponses
	{
		private Channels() { }

		public static AdvobotResult Display(
			IEnumerable<IGuildChannel> channels,
			[CallerMemberName] string caller = "")
		{
			var text = channels
				.OrderBy(x => x.Position)
				.Join("\n", x => $"{x.Position.ToString("00")}. {x.Name}");
			return Success(new EmbedWrapper
			{
				Title = Title.Format(ChannelsTitleChannelPositions, caller),
				Description = BigBlock.FormatInterpolated($"{text}"),
			});
		}
		public static AdvobotResult Moved(IGuildChannel channel, int position)
			=> Success(Default.Format(ChannelsMoved, channel, position));
		public static AdvobotResult DisplayOverwrites(
			IGuildChannel channel,
			IEnumerable<string> roleNames,
			IEnumerable<string> userNames)
		{
			var embed = new EmbedWrapper
			{
				Title = Title.Format(ChannelsTitleAllOverwrites, channel),
			};
			embed.TryAddField(ChannelsTitleAllOverwritesRoles, Default.FormatInterpolated($"{roleNames}"), false, out _);
			embed.TryAddField(ChannelsTitleAllOverwritesUsers, Default.FormatInterpolated($"{userNames}"), false, out _);
			return Success(embed);
		}
		public static AdvobotResult NoOverwriteFound(
			IGuildChannel channel,
			ISnowflakeEntity obj)
			=> Success(Default.Format(ChannelsNoOverwrite, obj, channel));
		public static AdvobotResult DisplayOverwrite(
			IGuildChannel channel,
			ISnowflakeEntity obj,
			IEnumerable<(string Name, string Value)> values)
		{
			var padLen = values.Max(x => x.Name.Length);
			var text = values.Join("\n", x => $"{x.Name.PadRight(padLen)} {x.Value}");
			return Success(new EmbedWrapper
			{
				Title = Title.Format(ChannelsTitleSingleOverwrite, obj, channel),
				Description = BigBlock.FormatInterpolated($"{text}"),
			});
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

			var flags = EnumUtils.GetFlagNames(permissions);
			return Success(Default.Format(format, flags, obj, channel));
		}
		public static AdvobotResult MismatchType(
			IGuildChannel input,
			IGuildChannel output)
			=> Failure(Default.Format(ChannelsFailedPermissionCopy, input, output));
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
			var value = obj?.Format() ?? ChannelsVariableAllOverwrites;
			return Success(Default.Format(ChannelsCopiedOverwrite, value, input, output));
		}
		public static AdvobotResult ClearedOverwrites(IGuildChannel channel, int count)
			=> Success(Default.Format(ChannelsClearedOverwrites, count, channel));
		public static AdvobotResult ModifiedNsfw(ITextChannel channel, bool nsfw)
			=> Success(Default.Format(ChannelsModifiedNsfw, channel, nsfw));
		public static AdvobotResult RemovedTopic(ITextChannel channel)
			=> Success(Default.Format(ChannelsRemovedTopic, channel));
		public static AdvobotResult ModifiedTopic(ITextChannel channel, string topic)
			=> Success(Default.Format(ChannelsModifiedTopic, channel, topic));
		public static AdvobotResult ModifiedLimit(IVoiceChannel channel, int limit)
			=> Success(Default.Format(ChannelsModifiedLimit, channel, limit));
		public static AdvobotResult ModifiedBitrate(IVoiceChannel channel, int bitrate)
			=> Success(Default.Format(ChannelsModifiedBitrate, channel, bitrate));
	}
}
