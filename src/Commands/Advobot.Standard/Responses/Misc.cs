using Advobot.Embeds;
using Advobot.Modules;
using Advobot.Services.Help;
using Advobot.Utilities;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using System.Reflection;
using System.Text;

using static Advobot.Resources.Responses;
using static Advobot.Utilities.FormattingUtils;

namespace Advobot.Standard.Responses;

public sealed partial class Misc : AdvobotResult
{
	private static readonly Type _Help = typeof(Commands.Misc.Help);

	public static AdvobotResult Help(IHelpModule module)
	{
		var sb = new StringBuilder()
			.AppendHeaderAndValue(MiscTitleAliases, module.Aliases.Join())
			.AppendHeaderAndValue(MiscTitleBasePermissions, FormatPreconditions(module.Preconditions))
			.AppendCategorySeparator()
			.AppendHeaderAndValue(MiscTitleDescription, module.Summary)
			.AppendCategorySeparator()
			.AppendHeaderAndValue(MiscTitleEnabledByDefault, module.EnabledByDefault)
			.AppendHeaderAndValue(MiscTitleAbleToBeToggled, module.AbleToBeToggled);

		if (module.Submodules.Count != 0)
		{
			var submodules = "\n" + module.Submodules
				.Select((x, i) => $"{i + 1}. {x.Name}")
				.Join("\n")
				.WithBigBlock()
				.Current;
			sb.AppendCategorySeparator()
				.AppendHeaderAndValue(MiscTitleSubmodules, submodules);
		}

		if (module.Commands.Count != 0)
		{
			var commands = "\n" + module.Commands.Select((x, i) =>
			{
				var parameters = x.Parameters.Select(FormatParameter).Join();
				var name = string.IsNullOrWhiteSpace(x.Name) ? "" : x.Name + " ";
				return $"{i + 1}. {name}({parameters})";
			}).Join("\n").WithBigBlock().Current;
			sb.AppendCategorySeparator()
				.AppendHeaderAndValue(MiscTitleCommands, commands);
		}

		return Success(CreateHelpEmbed(module.Aliases[0], sb.ToString()));
	}

	public static AdvobotResult Help(IHelpCommand command)
	{
		var sb = new StringBuilder()
			.AppendHeaderAndValue(MiscTitleAliases, command.Aliases.Select(x => x.WithBlock().Current).Join())
			.AppendHeaderAndValue(MiscTitleBasePermissions, FormatPreconditions(command.Preconditions))
			.AppendCategorySeparator()
			.AppendHeaderAndValue(MiscTitleDescription, command.Summary);

		var embed = CreateHelpEmbed(command.Aliases[0], sb.ToString());
		foreach (var parameter in command.Parameters)
		{
			var paramSb = new StringBuilder()
				.AppendHeaderAndValue(MiscTitleBasePermissions, FormatPreconditions(parameter.Preconditions))
				.AppendHeaderAndValue(MiscTitleDescription, parameter.Summary)
				.AppendHeaderAndValue(MiscTitleNamedArguments, parameter.NamedArguments.Join());

			embed.TryAddField(FormatParameter(parameter), paramSb.ToString(), true, out _);
		}
		return Success(embed);
	}

	public static AdvobotResult Help(IEnumerable<IHelpModule> entries, string category)
	{
		var title = MiscTitleCategoryCommands.Format(
			category.WithTitleCase()
		);
		var description = entries
			.Select(x => x.Name)
			.Join()
			.WithBigBlock()
			.Current;
		return Success(new EmbedWrapper
		{
			Title = title,
			Description = description,
		});
	}

	public static AdvobotResult Help(IEnumerable<string> categories, string prefix)
	{
		var description = MiscGeneralHelp.Format(
			GetPrefixedCommand(prefix, _Help, VariableCategoryParameter),
			GetPrefixedCommand(prefix, _Help, VariableCommandParameter)
		);
		var syntaxFieldValue = MiscBasicSyntax.Format(
			(VariableRequiredLeft + VariableRequiredRight).WithBlock(),
			(VariableOptionalLeft + VariableOptionalRight).WithBlock(),
			VariableOr.WithBlock()
		);
		return Success(new EmbedWrapper
		{
			Title = MiscTitleGeneralHelp,
			Description = description,
			Footer = new() { Text = MiscFooterHelp },
			Fields =
			[
				new()
				{
					Name = "Categories",
					Value = categories.Join().WithBigBlock(),
					IsInline = true,
				},
				new()
				{
					Name = MiscTitleBasicSyntax,
					Value = syntaxFieldValue,
					IsInline = false,
				},
				new()
				{
					Name = MiscTitleMentionSyntax,
					Value = MiscMentionSyntax,
					IsInline = true,
				},
				new()
				{
					Name = MiscTitleLinks,
					Value = MiscLinks.Format(
						Constants.REPO.WithNoMarkdown(),
						Constants.INVITE.WithNoMarkdown()
					),
					IsInline = false,
				},
			],
		});
	}

	public static AdvobotResult HelpInvalidPosition(IHelpModule module, int position)
	{
		return Failure(MiscInvalidHelpEntryNumber.Format(
			position.ToString().WithBlock(),
			module.Name.WithBlock()
		));
	}

	private static EmbedWrapper CreateHelpEmbed(string name, string entry)
	{
		return new()
		{
			Title = name,
			Description = entry,
			Footer = new() { Text = MiscFooterHelp, },
		};
	}

	private static string FormatParameter(IHelpParameter p)
	{
		var left = p.IsOptional ? VariableOptionalLeft : VariableRequiredLeft;
		var right = p.IsOptional ? VariableOptionalRight : VariableRequiredRight;
		return left + p.Name + right;
	}

	private static string FormatPreconditions(IEnumerable<IHelpPrecondition> preconditions)
	{
		if (!preconditions.Any())
		{
			return VariableNotApplicable;
		}
		if (preconditions.Any(x => x.Group is null))
		{
			return preconditions.Select(x => x.Summary).Join(VariableAnd);
		}

		var groups = preconditions
			.GroupBy(x => x.Group)
			.Select(g => g.Select(x => x.Summary).Join(VariableOr))
			.ToArray();
		if (groups.Length == 1)
		{
			return groups[0];
		}
		return groups.Select(x => $"({x})").Join(VariableAnd);
	}

	private static string FormatPreconditions(IEnumerable<IHelpParameterPrecondition> preconditions)
	{
		if (!preconditions.Any())
		{
			return VariableNotApplicable;
		}
		return preconditions.Select(x => x.Summary).Join(VariableAnd);
	}

	private static MarkdownString GetPrefixedCommand(
		string prefix,
		Type command,
		string args = "")
	{
		var attr = command.GetCustomAttribute<GroupAttribute>()
			?? throw new ArgumentException("Group is null.", nameof(command));
		return (prefix + attr.Prefix + (string.IsNullOrEmpty(args) ? "" : " ") + args).WithBlock();
	}
}

public sealed partial class Misc : AdvobotResult
{
	private static readonly IReadOnlyList<ActivityType> _Activities
		= Enum.GetValues<ActivityType>();
	private static readonly IReadOnlyList<GuildPermission> _Permissions
		= Enum.GetValues<GuildPermission>();
	private static readonly IReadOnlyList<UserStatus> _Statuses
		= Enum.GetValues<UserStatus>();

	public static AdvobotResult InfoBan(IBan ban)
	{
		var title = UsersTitleBanReason.Format(
			ban.User.Format().WithNoMarkdown()
		);
		return Success(new EmbedWrapper
		{
			Title = title,
			Description = ban.Reason ?? UsersNoBanReason,
		});
	}

	public static AdvobotResult InfoBot(IDiscordClient client)
	{
		var startTime = Constants.START.ToReadable();
		var runDuration = DateTime.UtcNow - Constants.START;
		var description = $"**Online Since:** `{startTime}` (`{runDuration:g}`)";
		if (client is BaseSocketClient socketClient)
		{
			description += $"\n**Latency:** `{socketClient.Latency}`";
		}
		if (client is DiscordShardedClient shardedClient)
		{
			description += $"**Shard Count:** `{shardedClient.Shards.Count}`";
		}

		return Success(new EmbedWrapper
		{
			Description = description,
			Author = client.CurrentUser.CreateAuthor(),
			Footer = new()
			{
				Text =
					$"Versions [Bot: {Constants.BOT_VERSION}] " +
					$"[Discord.Net: {Constants.DISCORD_NET_VERSION}]",
			},
		});
	}

	public static async Task<RuntimeResult> InfoChannel(IGuildChannel channel)
	{
		var userCount = channel is SocketGuildChannel sgc ? sgc.Users.Count : 0;
		var roles = new List<string>();
		var users = new List<string>();
		foreach (var o in channel.PermissionOverwrites)
		{
			if (o.TargetType == PermissionTarget.Role)
			{
				roles.Add(channel.Guild.GetRole(o.TargetId).Name);
			}
			else if (o.TargetType == PermissionTarget.User)
			{
				var user = await channel.Guild.GetUserAsync(o.TargetId).ConfigureAwait(false);
				users.Add(user.Username);
			}
		}

		var sb = new StringBuilder()
			.AppendTimeCreated(channel)
			.AppendCategorySeparator()
			.AppendHeaderAndValue(GetsTitlePosition, channel.Position)
			.AppendHeaderAndValue(GetsTitleUserCount, userCount);

		var embed = new EmbedWrapper
		{
			Description = sb.ToString(),
			Author = new()
			{
				Name = channel.Format(),
				IconUrl = channel.Guild.IconUrl,
			},
			Footer = new()
			{
				Text = GetsFooterChannel,
				IconUrl = channel.Guild.IconUrl,
			},
		};
		if (roles.Count > 0)
		{
			var fieldValue = roles.Join().WithBlock().Current;
			embed.TryAddField(GetsTitleRoles, fieldValue, false, out _);
		}
		if (users.Count > 0)
		{
			var fieldValue = users.Join().WithBlock().Current;
			embed.TryAddField(GetsTitleUsers, fieldValue, false, out _);
		}
		return Success(embed);
	}

	public static AdvobotResult InfoEmote(Emote emote)
	{
		var sb = new StringBuilder()
			.AppendTimeCreated(emote);

		if (emote is GuildEmote guildEmote)
		{
			sb
				.AppendCategorySeparator()
				.AppendHeaderAndValue(GetsTitleManaged, guildEmote.IsManaged)
				.AppendHeaderAndValue(GetsTitleColons, guildEmote.RequireColons)
				.AppendCategorySeparator()
				.AppendHeaderAndValue(GetsTitleRoles, guildEmote.RoleIds.Select(x => x.ToString()).Join());
		}

		return Success(new EmbedWrapper
		{
			Description = sb.ToString(),
			ThumbnailUrl = emote.Url,
			Author = new()
			{
				Name = ((IEmote)emote).Format(),
				IconUrl = emote.Url,
				Url = emote.Url,
			},
			Footer = new()
			{
				Text = GetsFooterEmote,
				IconUrl = emote.Url,
			},
		});
	}

	public static async Task<RuntimeResult> InfoGuild(IGuild guild)
	{
		var userCount = (await guild.GetUsersAsync(CacheMode.CacheOnly).ConfigureAwait(false)).Count;
		var owner = await guild.GetOwnerAsync().ConfigureAwait(false);

		int channels = 0, categories = 0, voice = 0, text = 0;
		foreach (var channel in await guild.GetChannelsAsync().ConfigureAwait(false))
		{
			++channels;
			if (channel is ICategoryChannel)
			{
				++categories;
			}
			if (channel is IVoiceChannel)
			{
				++voice;
			}
			if (channel is ITextChannel)
			{
				++text;
			}
		}
		int emotes = 0, local = 0, animated = 0, managed = 0;
		foreach (var emote in guild.Emotes)
		{
			++emotes;
			if (emote.IsManaged)
			{
				++managed;
			}
			if (emote.Animated)
			{
				++animated;
			}
			else
			{
				++local;
			}
		}

		var sb = new StringBuilder()
			.AppendTimeCreated(guild)
			.AppendCategorySeparator()
			.AppendHeaderAndValue(GetsTitleOwner, owner.Format())
			.AppendHeaderAndValue(GetsTitleUserCount, userCount)
			.AppendHeaderAndValue(GetsTitleRoleCount, guild.Roles.Count)
			.AppendHeaderAndValue(GetsTitleNotifications, guild.DefaultMessageNotifications)
			.AppendHeaderAndValue(GetsTitleVerification, guild.VerificationLevel)
			.AppendHeaderAndValue(GetsTitleVoiceRegion, guild.VoiceRegionId);

		var embed = new EmbedWrapper
		{
			Description = sb.ToString(),
			Color = owner.GetRoles().LastOrDefault(x => x.Color.RawValue != 0)?.Color,
			ThumbnailUrl = guild.IconUrl,
			Author = new()
			{
				Name = guild.Format(),
				IconUrl = guild.IconUrl,
			},
			Footer = new()
			{
				Text = GetsFooterGuild,
				IconUrl = guild.IconUrl,
			},
		};

		{
			var channelSb = new StringBuilder()
				.AppendHeaderAndValue(GetsTitleChannelCount, channels)
				.AppendHeaderAndValue(GetsTitleTextChannelCount, text)
				.AppendHeaderAndValue(GetsTitleVoiceChannelCount, voice)
				.AppendHeaderAndValue(GetsTitleCategoryChannelCount, categories)
				.AppendCategorySeparator()
				.AppendHeaderAndValue(GetsTitleDefaultChannel, (await guild.GetDefaultChannelAsync().ConfigureAwait(false)).Format())
				.AppendHeaderAndValue(GetsTitleAfkChannel, (await guild.GetAFKChannelAsync().ConfigureAwait(false)).Format())
				.AppendHeaderAndValue(GetsTitleSystemChannel, (await guild.GetSystemChannelAsync().ConfigureAwait(false)).Format())
				.AppendHeaderAndValue(GetsTitleEmbedChannel, (await guild.GetWidgetChannelAsync().ConfigureAwait(false)).Format());
			embed.TryAddField(GetsTitleChannelInfo, channelSb.ToString(), false, out _);
		}

		{
			var emoteSb = new StringBuilder()
				.AppendHeaderAndValue(GetsTitleEmoteCount, emotes)
				.AppendHeaderAndValue(GetsTitleAnimatedEmoteCount, animated)
				.AppendHeaderAndValue(GetsTitleLocalEmoteCount, local)
				.AppendHeaderAndValue(GetsTitleManagedEmoteCount, managed);
			embed.TryAddField(GetsTitleEmoteInfo, emoteSb.ToString(), false, out _);
		}

		{
			var fieldValue = "";
			if (guild.Features.Value != 0)
			{
				fieldValue += guild.Features.Value.ToString();
			}
			if (guild.Features.Experimental.Count > 0)
			{
				fieldValue += guild.Features.Experimental.Join();
			}

			if (!string.IsNullOrWhiteSpace(fieldValue))
			{
				fieldValue = fieldValue.WithBlock().Current;
				embed.TryAddField(GetsTitleFeatures, fieldValue, false, out _);
			}
		}
		return Success(embed);
	}

	public static async Task<RuntimeResult> InfoGuildUsers(IGuild guild)
	{
		var users = await guild.GetUsersAsync(CacheMode.CacheOnly).ConfigureAwait(false);
		var statuses = _Statuses.ToDictionary(x => x, _ => 0);
		var activities = _Activities.ToDictionary(x => x, _ => 0);
		int webhooks = 0, bots = 0, nickname = 0, voice = 0;
		foreach (var user in users)
		{
			++statuses[user.Status];
			foreach (var activity in user.Activities)
			{
				++activities[activity.Type];
			}
			if (user.IsWebhook)
			{
				++webhooks;
			}
			if (user.IsBot)
			{
				++bots;
			}
			if (user.Nickname != null)
			{
				++nickname;
			}
			if (user.VoiceChannel != null)
			{
				++voice;
			}
		}

		var sb = new StringBuilder()
			.AppendHeaderAndValue(GetsTitleUserCount, users.Count)
			.AppendHeaderAndValue(GetsTitleBotCount, bots)
			.AppendHeaderAndValue(GetsTitleWebhookCount, webhooks)
			.AppendHeaderAndValue(GetsTitleInVoiceCount, voice)
			.AppendHeaderAndValue(GetsTitleNicknameCount, nickname);

		var embed = new EmbedWrapper
		{
			Description = sb.ToString(),
			Author = new()
			{
				Name = guild.Format(),
				IconUrl = guild.IconUrl,
			},
			Footer = new()
			{
				Text = GetsFooterGuildUsers,
				IconUrl = guild.IconUrl,
			},
		};
		{
			var statusesSb = new StringBuilder();
			foreach (var (header, value) in statuses)
			{
				statusesSb.AppendHeaderAndValue(header.ToString(), value);
			}
			embed.TryAddField(GetsTitleStatuses, statusesSb.ToString(), false, out _);
		}
		{
			var activitiesSb = new StringBuilder();
			foreach (var (header, value) in activities)
			{
				activitiesSb.AppendHeaderAndValue(header.ToString(), value);
			}
			embed.TryAddField(GetsTitleActivities, activitiesSb.ToString(), false, out _);
		}
		return Success(embed);
	}

	public static AdvobotResult InfoInvite(IInviteMetadata invite)
	{
		var sb = new StringBuilder()
			.AppendTimeCreated(invite.Id, invite.CreatedAt.GetValueOrDefault())
			.AppendCategorySeparator()
			.AppendHeaderAndValue(GetsTitleCreator, invite.Inviter.Format())
			.AppendHeaderAndValue(GetsTitleChannel, invite.Channel.Format())
			.AppendHeaderAndValue(GetsTitleUses, invite.Uses ?? 0);

		return Success(new EmbedWrapper
		{
			Description = sb.ToString(),
			Author = new()
			{
				Name = invite.Format(),
				IconUrl = invite.Guild.IconUrl,
				Url = invite.Url,
			},
			Footer = new()
			{
				Text = GetsFooterInvite,
				IconUrl = invite.Guild.IconUrl,
			},
		});
	}

	public static async Task<RuntimeResult> InfoRole(IRole role)
	{
		var userCount = (await role.Guild.GetUsersAsync(CacheMode.CacheOnly).ConfigureAwait(false))
			.Count(x => x.RoleIds.Contains(role.Id));
		var permissions = _Permissions
			.Where(x => role.Permissions.Has(x))
			.Select(x => x.ToString("F"))
			.ToArray();

		var sb = new StringBuilder()
			.AppendTimeCreated(role)
			.AppendCategorySeparator()
			.AppendHeaderAndValue(GetsTitlePosition, role.Position)
			.AppendHeaderAndValue(GetsTitleUserCount, userCount)
			.AppendHeaderAndValue(GetsTitleColor, $"#{role.Color.RawValue:X6}")
			.AppendCategorySeparator()
			.AppendHeaderAndValue(GetsTitleHoisted, role.IsHoisted)
			.AppendHeaderAndValue(GetsTitleManaged, role.IsManaged)
			.AppendHeaderAndValue(GetsTitleMentionable, role.IsMentionable);

		var embed = new EmbedWrapper
		{
			Description = sb.ToString(),
			Color = role.Color,
			Author = new() { Name = role.Format(), },
			Footer = new() { Text = GetsFooterRole, },
		};
		if (permissions.Length > 0)
		{
			var fieldValue = permissions.Join().WithBlock().Current;
			embed.TryAddField(GetsTitlePermissions, fieldValue, false, out _);
		}
		return Success(embed);
	}

	public static AdvobotResult InfoShards(DiscordShardedClient client)
	{
		var description = client.Shards.Select(shard =>
		{
			var statusEmoji = shard.ConnectionState switch
			{
				ConnectionState.Disconnected => Constants.DENIED,
				ConnectionState.Disconnecting => Constants.DENIED,
				ConnectionState.Connected => Constants.ALLOWED,
				ConnectionState.Connecting => Constants.ALLOWED,
				_ => Constants.UNKNOWN,
			};
			return $"Shard `{shard.ShardId}`: `{statusEmoji} ({shard.Latency}ms)`";
		}).Join("\n");
		return Success(new EmbedWrapper
		{
			Description = description,
			Author = client.CurrentUser.CreateAuthor(),
			Footer = new()
			{
				Text = GetsFooterShards,
				IconUrl = client.CurrentUser.GetAvatarUrl(),
			},
		});
	}

	public static async Task<RuntimeResult> InfoUser(IUser user)
	{
		var sb = new StringBuilder()
			.AppendTimeCreated(user)
			.AppendCategorySeparator()
			.AppendHeaderAndValue(GetsTitleActivity, user.Activities.Select(x => x.Format()).Join("\n"))
			.AppendHeaderAndValue(GetsTitleStatus, user.Status);

		var embed = new EmbedWrapper
		{
			ThumbnailUrl = user.GetAvatarUrl(),
			Author = user.CreateAuthor(),
			Footer = new()
			{
				Text = GetsFooterUser,
				IconUrl = user.GetAvatarUrl(),
			},
		};

		// User is not from a guild so we can't get any more information about them
		if (user is not IGuildUser guildUser)
		{
			embed.Description = sb.ToString();
			return Success(embed);
		}

		var optionalSb = new StringBuilder();
		if (guildUser.Nickname is string nickname)
		{
			sb.AppendHeaderAndValue(GetsTitleNickname, nickname.EscapeBackTicks());
		}
		if (guildUser.JoinedAt is DateTimeOffset dto)
		{
			//If cachemode is allow download this can take ages
			var joinNum = (await guildUser.Guild.GetUsersAsync(CacheMode.CacheOnly).ConfigureAwait(false))
				.Count(x => x.JoinedAt < guildUser.JoinedAt);
			sb.AppendHeaderAndValue(GetsTitleJoined, GetsJoinedAt.Format(
				dto.UtcDateTime.ToReadable().WithNoMarkdown(),
				joinNum.ToString().WithNoMarkdown()
			));
		}
		if (guildUser.VoiceChannel is IVoiceChannel vc)
		{
			sb.AppendHeaderAndValue(GetsTitleVoiceChannel, vc.Format());
		}
		if (optionalSb.Length > 0)
		{
			sb.AppendCategorySeparator().Append(optionalSb);
		}
		embed.Description = sb.ToString();

		async Task<IReadOnlyCollection<T>> GetChannelsAsync<T>(
			Func<IGuild, Task<IReadOnlyCollection<T>>> getter,
			Func<ChannelPermissions, bool> permCheck)
			where T : IGuildChannel
		{
			var channels = await getter(guildUser.Guild).ConfigureAwait(false);
			var ordered = channels.OrderBy(x => x.Position);
			var valid = ordered.Where(x => permCheck(guildUser.GetPermissions(x)));
			return valid.ToArray();
		}

		var roles = guildUser.GetRoles();
		var textChannels = await GetChannelsAsync(
			x => x.GetTextChannelsAsync(),
			x => x.ViewChannel
		).ConfigureAwait(false);
		var voiceChannels = await GetChannelsAsync(
			x => x.GetVoiceChannelsAsync(),
			x => x.ViewChannel && x.Connect
		).ConfigureAwait(false);

		if (roles.Count > 0)
		{
			var fieldValue = roles.Select(x => x.Name).Join().WithBigBlock().Current;
			embed.TryAddField(GetsTitleRoles, fieldValue, false, out _);
			embed.Color = roles.LastOrDefault(x => x.Color.RawValue != 0)?.Color;
		}
		if (textChannels.Count > 0)
		{
			var fieldValue = textChannels.Select(x => x.Name).Join().WithBigBlock().Current;
			embed.TryAddField(GetsTitleTextChannels, fieldValue, false, out _);
		}
		if (voiceChannels.Count > 0)
		{
			var fieldValue = voiceChannels.Select(x => x.Name).Join().WithBigBlock().Current;
			embed.TryAddField(GetsTitleVoiceChannels, fieldValue, false, out _);
		}

		return Success(embed);
	}

	public static AdvobotResult InfoWebhook(IWebhook webhook)
	{
		var sb = new StringBuilder()
			.AppendTimeCreated(webhook)
			.AppendCategorySeparator()
			.AppendHeaderAndValue(GetsTitleCreator, webhook.Creator.Format())
			.AppendHeaderAndValue(GetsTitleChannel, webhook.Channel.Format());

		return Success(new EmbedWrapper
		{
			Description = sb.ToString(),
			ThumbnailUrl = webhook.GetAvatarUrl(),
			Author = new()
			{
				Name = webhook.Format(),
				IconUrl = webhook.GetAvatarUrl(),
				Url = webhook.GetAvatarUrl(),
			},
			Footer = new()
			{
				Text = GetsFooterWebhook,
				IconUrl = webhook.GetAvatarUrl(),
			},
		});
	}
}

internal static class ResponseUtils
{
	public static StringBuilder AppendCategorySeparator(this StringBuilder sb)
		=> sb.Append('\n');

	public static StringBuilder AppendHeaderAndValue(
		this StringBuilder sb,
		string header,
		object? value)
	{
		var valStr = value?.ToString();
		if (string.IsNullOrWhiteSpace(valStr))
		{
			return sb;
		}

		if (sb.Length > 0)
		{
			sb.Append('\n');
		}

		return sb
			.Append(header.WithTitleCaseAndColon())
			.Append(' ')
			.Append(value);
	}

	public static StringBuilder AppendTimeCreated(
		this StringBuilder sb,
		ISnowflakeEntity entity)
		=> sb.AppendTimeCreated(entity.Id.ToString(), entity.CreatedAt.UtcDateTime);

	public static StringBuilder AppendTimeCreated(
		this StringBuilder sb,
		string id,
		DateTimeOffset dt)
	{
		var diff = (DateTimeOffset.UtcNow - dt).TotalDays;
		return sb
			.AppendHeaderAndValue("Id", id)
			.AppendHeaderAndValue("Created At", $"{dt.DateTime.ToReadable()} ({diff:0.00} days ago)");
	}
}