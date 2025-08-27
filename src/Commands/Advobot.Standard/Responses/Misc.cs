using Advobot.Embeds;
using Advobot.Modules;
using Advobot.Services.Help;
using Advobot.Utilities;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using System.Reflection;

using static Advobot.Resources.Responses;
using static Advobot.Utilities.FormattingUtils;

namespace Advobot.Standard.Responses;

public sealed partial class Misc : AdvobotResult
{
	private static readonly Type _Commands = typeof(Commands.Misc.Commands);
	private static readonly Type _Help = typeof(Commands.Misc.Help);

	public static AdvobotResult CommandsCategory(
		IEnumerable<IHelpModule> entries,
		string category)
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

	public static AdvobotResult CommandsGeneral(
		IEnumerable<string> categories,
		string prefix)
	{
		var description = MiscGeneralCommandInfo.Format(
			GetPrefixedCommand(prefix, _Commands, VariableCategoryParameter),
			categories.Join().WithBigBlock()
		);
		return Success(new EmbedWrapper
		{
			Title = MiscTitleCategories,
			Description = description,
		});
	}

	public static AdvobotResult Help(IHelpModule module)
	{
		var info = new List<List<(string, string)>>()
		{
			new()
			{
				(MiscTitleAliases, module.Aliases.Join()),
				(MiscTitleBasePermissions, FormatPreconditions(module.Preconditions)),
			},
			new()
			{
				(MiscTitleDescription, module.Summary),
			},
			new()
			{
				(MiscTitleEnabledByDefault, module.EnabledByDefault.ToString()),
				(MiscTitleAbleToBeToggled, module.AbleToBeToggled.ToString()),
			},
		};

		if (module.Submodules.Count != 0)
		{
			var submodules = "\n" + module.Submodules
				.Select((x, i) => $"{i + 1}. {x.Name}")
				.Join("\n")
				.WithBigBlock()
				.Current;
			info.Add([(MiscTitleSubmodules, submodules)]);
		}

		if (module.Commands.Count != 0)
		{
			var commands = "\n" + module.Commands.Select((x, i) =>
			{
				var parameters = x.Parameters.Select(FormatParameter).Join();
				var name = string.IsNullOrWhiteSpace(x.Name) ? "" : x.Name + " ";
				return $"{i + 1}. {name}({parameters})";
			}).Join("\n").WithBigBlock().Current;
			info.Add([(MiscTitleCommands, commands)]);
		}

		return Success(CreateHelpEmbed(module.Aliases[0], Format(info)));
	}

	public static AdvobotResult Help(IHelpModule module, int position)
	{
		if (module.Commands.Count < position)
		{
			return Failure(MiscInvalidHelpEntryNumber.Format(
				position.ToString().WithBlock(),
				module.Name.WithBlock()
			));
		}
		var command = module.Commands[position - 1];

		var info = new List<List<(string, string)>>()
		{
			new()
			{
				(MiscTitleAliases, command.Aliases.Select(x => x.WithBlock().Current).Join()),
				(MiscTitleBasePermissions, FormatPreconditions(command.Preconditions)),
			},
			new()
			{
				(MiscTitleDescription, command.Summary),
			},
		};

		var embed = CreateHelpEmbed(command.Aliases[0], Format(info));
		foreach (var parameter in command.Parameters)
		{
			var paramInfo = new List<(string, string)>()
			{
				(MiscTitleBasePermissions, FormatPreconditions(parameter.Preconditions)),
				(MiscTitleDescription, parameter.Summary),
				(MiscTitleNamedArguments, parameter.NamedArguments.Join()),
			};

			embed.TryAddField(FormatParameter(parameter), Format(paramInfo), true, out _);
		}
		return Success(embed);
	}

	public static AdvobotResult HelpGeneral(string prefix)
	{
		var description = MiscGeneralHelp.Format(
			GetPrefixedCommand(prefix, _Commands),
			GetPrefixedCommand(prefix, _Help, VariableCategoryParameter)
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
					Name = MiscTitleBasicSyntax,
					Value = syntaxFieldValue,
					IsInline = true,
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
		return $"{prefix}{attr.Prefix} {args}".WithBlock();
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

		var info = new List<List<(string, string)>>()
		{
			TimeCreated(channel),
			new()
			{
				(GetsTitlePosition, channel.Position.ToString()),
				(GetsTitleUserCount, userCount.ToString()),
			},
		};

		var embed = new EmbedWrapper
		{
			Description = Format(info),
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
		var info = new List<List<(string, string)>>()
		{
			TimeCreated(emote),
		};

		if (emote is GuildEmote guildEmote)
		{
			info.Add(
			[
				(GetsTitleManaged, guildEmote.IsManaged.ToString()),
				(GetsTitleColons, guildEmote.RequireColons.ToString()),
			]);
			info.Add(
			[
				(GetsTitleRoles, guildEmote.RoleIds.Select(x => x.ToString()).Join()),
			]);
		}

		return Success(new EmbedWrapper
		{
			Description = Format(info),
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

		var info = new List<List<(string, string)>>()
		{
			TimeCreated(guild),
			new()
			{
				(GetsTitleOwner, owner.Format()),
				(GetsTitleUserCount, userCount.ToString()),
				(GetsTitleRoleCount, guild.Roles.Count.ToString()),
				(GetsTitleNotifications, guild.DefaultMessageNotifications.ToString()),
				(GetsTitleVerification, guild.VerificationLevel.ToString()),
				(GetsTitleVoiceRegion, guild.VoiceRegionId),
			},
		};

		var embed = new EmbedWrapper
		{
			Description = Format(info),
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
			var channelInfo = new List<List<(string, string)>>()
			{
				new()
				{
					(GetsTitleChannelCount, channels.ToString()),
					(GetsTitleTextChannelCount, text.ToString()),
					(GetsTitleVoiceChannelCount, voice.ToString()),
					(GetsTitleCategoryChannelCount, categories.ToString()),
				},
				new()
				{
					(GetsTitleDefaultChannel, (await guild.GetDefaultChannelAsync().ConfigureAwait(false)).Format()),
					(GetsTitleAfkChannel, (await guild.GetAFKChannelAsync().ConfigureAwait(false)).Format()),
					(GetsTitleSystemChannel, (await guild.GetSystemChannelAsync().ConfigureAwait(false)).Format()),
					(GetsTitleEmbedChannel, (await guild.GetWidgetChannelAsync().ConfigureAwait(false)).Format()),
				},
			};
			embed.TryAddField(GetsTitleChannelInfo, Format(channelInfo), false, out _);
		}

		{
			var emoteInfo = new List<(string, string)>()
			{
				(GetsTitleEmoteCount, emotes.ToString()),
				(GetsTitleAnimatedEmoteCount, animated.ToString()),
				(GetsTitleLocalEmoteCount, local.ToString()),
				(GetsTitleManagedEmoteCount, managed.ToString()),
			};
			embed.TryAddField(GetsTitleEmoteInfo, Format(emoteInfo), false, out _);
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

		var info = new List<List<(string, string)>>()
		{
			new()
			{
				(GetsTitleUserCount, users.Count.ToString()),
				(GetsTitleBotCount, bots.ToString()),
				(GetsTitleWebhookCount, webhooks.ToString()),
				(GetsTitleInVoiceCount, voice.ToString()),
				(GetsTitleNicknameCount, nickname.ToString()),
			},
		};

		var embed = new EmbedWrapper
		{
			Description = Format(info),
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
			var value = statuses
				.Select(kvp => (kvp.Key.ToString(), kvp.Value.ToString()));
			embed.TryAddField(GetsTitleStatuses, Format(value), false, out _);
		}
		{
			var value = activities
				.Select(kvp => (kvp.Key.ToString(), kvp.Value.ToString()));
			embed.TryAddField(GetsTitleActivities, Format(value), false, out _);
		}
		return Success(embed);
	}

	public static AdvobotResult InfoInvite(IInviteMetadata invite)
	{
		var info = new List<List<(string, string)>>()
		{
			TimeCreated(invite.Id, invite.CreatedAt.GetValueOrDefault()),
			new()
			{
				(GetsTitleCreator, invite.Inviter.Format()),
				(GetsTitleChannel, invite.Channel.Format()),
				(GetsTitleUses, (invite.Uses ?? 0).ToString()),
			},
		};

		return Success(new EmbedWrapper
		{
			Description = Format(info),
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

		var info = new List<List<(string, string)>>()
		{
			TimeCreated(role),
			new()
			{
				(GetsTitlePosition, role.Position.ToString()),
				(GetsTitleUserCount, userCount.ToString()),
				(GetsTitleColor, $"#{role.Color.RawValue:X6}"),
			},
			new()
			{
				(GetsTitleHoisted, role.IsHoisted.ToString()),
				(GetsTitleManaged, role.IsManaged.ToString()),
				(GetsTitleMentionable, role.IsMentionable.ToString()),
			},
		};

		var embed = new EmbedWrapper
		{
			Description = Format(info),
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
		var info = new List<List<(string, string)>>()
		{
			TimeCreated(user),
			new()
			{
				(GetsTitleActivity, user.Activities.Select(x => x.Format()).Join("\n")),
				(GetsTitleStatus, user.Status.ToString()),
			},
		};

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
			embed.Description = Format(info);
			return Success(embed);
		}

		var guildInfo = new List<(string, string)>();
		if (guildUser.Nickname is string nickname)
		{
			guildInfo.Add((GetsTitleNickname, nickname.EscapeBackTicks()));
		}
		if (guildUser.JoinedAt is DateTimeOffset dto)
		{
			//If cachemode is allow download this can take ages
			var joinNum = (await guildUser.Guild.GetUsersAsync(CacheMode.CacheOnly).ConfigureAwait(false))
				.Count(x => x.JoinedAt < guildUser.JoinedAt);
			guildInfo.Add((GetsTitleJoined, GetsJoinedAt.Format(
				dto.UtcDateTime.ToReadable().WithNoMarkdown(),
				joinNum.ToString().WithNoMarkdown()
			)));
		}
		if (guildUser.VoiceChannel is IVoiceChannel vc)
		{
			guildInfo.Add((GetsTitleVoiceChannel, vc.Format()));
		}
		embed.Description = Format(info);

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
		var info = new List<List<(string, string)>>()
		{
			TimeCreated(webhook),
			new()
			{
				(GetsTitleCreator, webhook.Creator.Format()),
				(GetsTitleChannel, webhook.Channel.Format()),
			},
		};

		return Success(new EmbedWrapper
		{
			Description = Format(info),
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

	private static string Format(IEnumerable<IEnumerable<(string, string)>> info)
	{
		return info
			.Where(x => x.Any())
			.Select(Format)
			.Where(x => !string.IsNullOrWhiteSpace(x))
			.Join("\n\n");
	}

	private static string Format(IEnumerable<(string, string)> info)
	{
		return info.Select(x =>
		{
			var (title, value) = x;
			return string.IsNullOrWhiteSpace(value)
				? string.Empty
				: $"{title.WithTitleCaseAndColon()} {value}";
		}).Join("\n");
	}

	private static List<(string, string)> TimeCreated(ISnowflakeEntity e)
		=> TimeCreated(e.Id.ToString(), e.CreatedAt.UtcDateTime);

	private static List<(string, string)> TimeCreated(string id, DateTimeOffset dt)
	{
		var diff = (DateTimeOffset.UtcNow - dt).TotalDays;
		return
		[
			("Id", id),
			("Created At", $"{dt.DateTime.ToReadable()} ({diff:0.00} days ago)"),
		];
	}
}