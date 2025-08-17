using Advobot.Embeds;
using Advobot.Modules;
using Advobot.Utilities;

using Discord;

using System.Runtime.CompilerServices;

using static Advobot.Resources.Responses;

namespace Advobot.Standard.Responses;

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
			(obj?.Format() ?? VariableAll).WithBlock(),
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
			.Select(x => $"{x.Position:00}. {x.Name}")
			.Join("\n")
			.WithBigBlock()
			.Current;
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
			.ToDictionary(x => x.Key.ToString(), x => x.Value)
			.FormatPermissionList()
			.WithBigBlock()
			.Current;
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

		var rolesValue = roleNames.Join().WithBigBlock().Current;
		embed.TryAddField(ChannelsTitleAllOverwritesRoles, rolesValue, false, out _);
		var usersValue = userNames.Join().WithBigBlock().Current;
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
			permissions.ToString("F").WithBlock(),
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