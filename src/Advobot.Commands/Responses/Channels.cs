﻿using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Advobot.Classes;
using Advobot.Classes.Results;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;

namespace Advobot.Commands.Responses
{
	public sealed class Channels : CommandResponses
	{
		private Channels() { }

		public static AdvobotResult Created(IGuildChannel channel)
			=> Success(Default.Format(strings.Responses_Channels_Created, channel));
		public static AdvobotResult SoftDeleted(IGuildChannel channel)
			=> Success(Default.Format(strings.Responses_Channels_SoftDeleted, channel));
		public static AdvobotResult Deleted(IGuildChannel channel)
			=> Success(Default.Format(strings.Responses_Channels_Deleted, channel));
		public static AdvobotResult DisplayMany(IEnumerable<IGuildChannel> channels, [CallerMemberName] string caller = "")
		{
			return Success(new EmbedWrapper
			{
				Title = Title.Format(strings.Responses_Channels_Positions_Title, caller),
				Description = BigBlock.FormatInterpolated($"{channels.Join("\n", x => $"{x.Position.ToString("00")}. {x.Name}")}"),
			});
		}
		public static AdvobotResult Moved(IGuildChannel channel, int position)
			=> Success(Default.Format(strings.Responses_Channels_Moved, channel, position));
		public static AdvobotResult DisplayOverwrites(IGuildChannel channel, IEnumerable<string> roleNames, IEnumerable<string> userNames)
		{
			var embed = new EmbedWrapper { Title = Title.Format(strings.Responses_Channels_AllOverwrites_Title, channel), };
			embed.TryAddField("Roles", Default.FormatInterpolated($"{roleNames}"), false, out _);
			embed.TryAddField("Users", Default.FormatInterpolated($"{userNames}"), false, out _);
			return Success(embed);
		}
		public static AdvobotResult NoOverwriteFound(IGuildChannel channel, ISnowflakeEntity obj)
			=> Success(Default.FormatInterpolated($"There are no overwrites for {obj} on {channel}."));
		public static AdvobotResult DisplayOverwrite(IGuildChannel channel, ISnowflakeEntity obj, IEnumerable<(string Name, string Value)> values)
		{
			var padLen = values.Max(x => x.Name.Length);
			return Success(new EmbedWrapper
			{
				Title = Title.FormatInterpolated($"Overwrite On {channel}"),
				Description = Default.FormatInterpolated($"{obj}\n") + BigBlock.FormatInterpolated($"{values.Join("\n", x => $"{x.Name.PadRight(padLen)} {x.Value}")}"),
			});
		}
		public static AdvobotResult ModifiedOverwrite(IGuildChannel channel, ISnowflakeEntity obj, ChannelPermission permissions, PermValue action)
			=> Success(Default.FormatInterpolated($"Successfully {GetAction(action)} {EnumUtils.GetFlagNames(permissions)} for {obj} on {channel}."));
		public static AdvobotResult MismatchType(IGuildChannel input, IGuildChannel output)
			=> Failure($"Failed to copy channel permissions because channels {input} and {output} are not the same type.");
		public static AdvobotResult CopiedOverwrites(IGuildChannel input, IGuildChannel output, ISnowflakeEntity? obj, IReadOnlyCollection<Overwrite> overwrites)
			=> overwrites.Count > 0
				? Success(Default.FormatInterpolated($"Successfully copied {obj?.Format() ?? "All"} from {input} to {output}"))
				: Success($"There are no matching overwrite{(obj == null ? "" : "s")} to copy.").WithTime(DefaultTime);
		public static AdvobotResult ClearedOverwrites(IGuildChannel channel, int count)
			=> Success(Default.FormatInterpolated($"Successfully removed {count} overwrites from {channel}.")).WithTime(DefaultTime);
		public static AdvobotResult ModifiedNsfw(IGuildChannel channel, bool nsfw)
			=> Success(Default.FormatInterpolated($"Successfully {(nsfw ? "un" : "")}marked {channel} as NSFW."));
		public static AdvobotResult ModifiedName(string old, string name)
			=> Success(Default.FormatInterpolated($"Successfully changed the name of {old} to {name}."));
		public static AdvobotResult ModifiedTopic(IGuildChannel channel, string topic)
			=> Success(Default.FormatInterpolated($"Successfully changed the topic for {channel} to {topic ?? "Nothing"}."));
		public static AdvobotResult ModifiedLimit(IVoiceChannel channel, int limit)
			=> Success(Default.FormatInterpolated($"Successfully changed the user limit for {channel} to {limit}."));
		public static AdvobotResult ModifiedBitRate(IVoiceChannel channel, int bitrate)
			=> Success(Default.FormatInterpolated($"Successfully changed the bitrate for {channel} to {bitrate}kbps."));
	}
}
