using Advobot.Embeds;
using Advobot.Modules;
using Advobot.Utilities;

using Discord;
using Discord.Rest;
using Discord.WebSocket;

using System.Text;

using static Advobot.Resources.Responses;
using static Advobot.Utilities.FormattingUtils;

namespace Advobot.Standard.Responses;

public sealed partial class Misc : AdvobotResult
{
	/*
	private static readonly Type _Help = typeof(Commands.Misc.Help);

	public static AdvobotResult Help(IImmutableCommand module)
	{
		var enabled = module.EnabledByDefault.ToString();
		if (!module.AbleToBeToggled)
		{
			enabled += MiscCannotBeDisabled;
		}

		var sb = new StringBuilder()
			.AppendHeaderAndValue(MiscTitleAliases, module.Aliases.Join())
			.AppendHeaderAndValue(MiscTitleBasePermissions, FormatPreconditions(module.Preconditions))
			.AppendHeaderAndValue(MiscTitleEnabledByDefault, enabled);

		sb.AppendCategorySeparator()
			.AppendHeaderAndValue(MiscTitleDescription, module.Summary);

		if (module.Submodules.Count != 0)
		{
			var submodules = module.Submodules
				.Select((x, i) => $"{i + 1}. {x.Name}")
				.Join("\n")
				.WithBigBlock();
			sb.AppendCategorySeparator()
				.AppendHeaderAndValue(MiscTitleSubmodules, submodules);
		}

		if (module.Commands.Count != 0)
		{
			var commands = module.Commands.Select((x, i) =>
			{
				var parameters = x.Parameters.Select(FormatParameter).Join();
				var name = string.IsNullOrWhiteSpace(x.Name) ? "" : x.Name + " ";
				return $"{i + 1}. {name}({parameters})";
			}).Join("\n").WithBigBlock();
			sb.AppendCategorySeparator()
				.AppendHeaderAndValue(MiscTitleCommands, commands);
		}

		return Success(CreateHelpEmbed(module.Aliases[0], sb.ToString()));
	}

	public static AdvobotResult Help(IImmutableCommand command)
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

	public static AdvobotResult Help(IEnumerable<IImmutableCommand> entries, string category)
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
			Footer = new()
			{
				Text = MiscFooterHelp,
			},
			Fields =
			[
				new()
				{
					Name = MiscTitleCategories,
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
			Footer = new()
			{
				Text = MiscFooterHelp,
			},
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
	*/
}

public sealed partial class Misc : AdvobotResult
{
	private static readonly IReadOnlyList<GuildPermission> _Permissions
		= Enum.GetValues<GuildPermission>();

	public static AdvobotResult InfoBan(IBan ban)
	{
		var sb = new StringBuilder()
			.AppendHeaderAndValue(GetsTitleUser, ban.User.Format())
			.AppendHeaderAndValue(GetsTitleReason, ban.Reason ?? UsersNoBanReason);
		return Success(new EmbedWrapper
		{
			Description = sb.ToString(),
			Author = ban.User.CreateAuthor(),
			Footer = new()
			{
				Text = GetsFooterBan,
				IconUrl = ban.User.GetAvatarUrl(),
			},
		});
	}

	public static AdvobotResult InfoBot(IDiscordClient client)
	{
		var runDuration = DateTime.UtcNow - Constants.START;
		var ts = new TimeSpan(runDuration.Days, runDuration.Hours, runDuration.Minutes, runDuration.Seconds);
		var sb = new StringBuilder()
			.AppendHeaderAndValue(GetsTitleOnline, GetsValueOnline.Format(
				Constants.START.ToReadable().WithNoMarkdown(),
				ts.ToString("g").WithNoMarkdown()
			));
		if (client is BaseSocketClient socketClient)
		{
			sb.AppendHeaderAndValue(GetsTitleLatency, GetsValueLatency.Format(
				socketClient.Latency.ToString().WithNoMarkdown()
			));
		}
		if (client is DiscordShardedClient shardedClient)
		{
			sb.AppendCategorySeparator();
			foreach (var shard in shardedClient.Shards)
			{
				var statusEmoji = shard.ConnectionState switch
				{
					ConnectionState.Disconnected => Constants.DENIED,
					ConnectionState.Disconnecting => Constants.DENIED,
					ConnectionState.Connected => Constants.ALLOWED,
					ConnectionState.Connecting => Constants.ALLOWED,
					_ => Constants.UNKNOWN,
				};
				sb.AppendHeaderAndValue(GetsTitleShard.Format(
					shard.ShardId.ToString().WithNoMarkdown()
				), GetsValueShard.Format(
					statusEmoji.WithNoMarkdown(),
					shard.Latency.ToString().WithNoMarkdown()
				));
			}
		}

		return Success(new EmbedWrapper
		{
			Description = sb.ToString(),
			Author = client.CurrentUser.CreateAuthor(),
			Footer = new()
			{
				Text = GetsFooterBot.Format(
					Constants.BOT_VERSION.WithNoMarkdown(),
					Constants.DISCORD_NET_VERSION.WithNoMarkdown()
				),
				IconUrl = client.CurrentUser.GetAvatarUrl(),
			},
		});
	}

	public static async Task<AdvobotResult> InfoChannel(IGuildChannel channel)
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
				IconUrl = channel.Guild.IconUrl,
				Name = channel.Format(),
				Url = channel.Guild.IconUrl,
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
			Author = new()
			{
				IconUrl = emote.Url,
				Name = ((IEmote)emote).Format(),
				Url = emote.Url,
			},
			Footer = new()
			{
				Text = GetsFooterEmote,
				IconUrl = emote.Url,
			},
		});
	}

	public static async Task<AdvobotResult> InfoGuild(IGuild guild)
	{
		var users = await guild.GetUsersAsync(CacheMode.CacheOnly).ConfigureAwait(false);
		var owner = await guild.GetOwnerAsync().ConfigureAwait(false);

		int webhooks = 0, bots = 0;
		foreach (var user in users)
		{
			if (user.IsWebhook)
			{
				++webhooks;
			}
			if (user.IsBot)
			{
				++bots;
			}
		}
		int channels = 0, category = 0, voice = 0, text = 0;
		foreach (var channel in await guild.GetChannelsAsync().ConfigureAwait(false))
		{
			++channels;
			if (channel is ICategoryChannel)
			{
				++category;
			}
			else if (channel is IVoiceChannel)
			{
				++voice;
			}
			else if (channel is ITextChannel)
			{
				++text;
			}
		}
		int emotes = 0, animated = 0;
		foreach (var emote in guild.Emotes)
		{
			++emotes;
			if (emote.Animated)
			{
				++animated;
			}
		}

		var sb = new StringBuilder()
			.AppendTimeCreated(guild)
			.AppendCategorySeparator()
			.AppendHeaderAndValue(GetsTitleOwner, owner.Format());

		var embed = new EmbedWrapper
		{
			Description = sb.ToString(),
			Color = owner.GetRoles().LastOrDefault(x => x.Color.RawValue != 0)?.Color,
			Author = new()
			{
				IconUrl = guild.IconUrl,
				Name = guild.Format(),
				Url = guild.IconUrl,
			},
			Footer = new()
			{
				Text = GetsFooterGuild,
				IconUrl = guild.IconUrl,
			},
		};

		{
			var countsSb = new StringBuilder()
				.AppendHeaderAndValue(GetsTitleUserCount, GetsValueUserCount.Format(
					users.Count.ToString().WithNoMarkdown(),
					bots.ToString().WithNoMarkdown(),
					webhooks.ToString().WithNoMarkdown()
				))
				.AppendHeaderAndValue(GetsTitleRoleCount, guild.Roles.Count)
				.AppendHeaderAndValue(GetsTitleChannelCount, GetsValueChannelCount.Format(
					channels.ToString().WithNoMarkdown(),
					text.ToString().WithNoMarkdown(),
					voice.ToString().WithNoMarkdown(),
					category.ToString().WithNoMarkdown()
				))
				.AppendHeaderAndValue(GetsTitleEmoteCount, GetsValueEmoteCount.Format(
					emotes.ToString().WithNoMarkdown(),
					animated.ToString().WithNoMarkdown()
				));
			embed.TryAddField(GetsTitleCounts, countsSb.ToString(), false, out _);
		}

		{
			var channelSb = new StringBuilder()
				.AppendHeaderAndValue(GetsTitleDefaultChannel, (await guild.GetDefaultChannelAsync().ConfigureAwait(false)).Format())
				.AppendHeaderAndValue(GetsTitleAfkChannel, (await guild.GetAFKChannelAsync().ConfigureAwait(false)).Format())
				.AppendHeaderAndValue(GetsTitleSystemChannel, (await guild.GetSystemChannelAsync().ConfigureAwait(false)).Format());
			embed.TryAddField(GetsTitleChannels, channelSb.ToString(), false, out _);
		}

		{
			var flagsSb = new StringBuilder()
				.AppendHeaderAndValue(GetsTitleNotifications, guild.DefaultMessageNotifications)
				.AppendHeaderAndValue(GetsTitleVerification, guild.VerificationLevel);
			if (guild.Features.Value != 0)
			{
				flagsSb.AppendHeaderAndValue(GetsTitleNormalFeatures, guild.Features.Value);
			}
			if (guild.Features.Experimental.Count > 0)
			{
				flagsSb.AppendHeaderAndValue(GetsTitleExperimentalFeatures, guild.Features.Experimental.Join());
			}

			if (flagsSb.Length > 0)
			{
				embed.TryAddField(GetsTitleFlags, flagsSb.ToString(), false, out _);
			}
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

		var iconUrl = default(string?);
		if (invite is RestInvite restInvite)
		{
			iconUrl = restInvite.PartialGuild.IconUrl;
		}

		return Success(new EmbedWrapper
		{
			Description = sb.ToString(),
			Author = new()
			{
				IconUrl = iconUrl,
				Name = invite.Format(),
				Url = iconUrl,
			},
			Footer = new()
			{
				Text = GetsFooterInvite,
				IconUrl = iconUrl,
			},
		});
	}

	public static AdvobotResult InfoNotFound()
		=> Failure(GetsNotFound);

	public static async Task<AdvobotResult> InfoRole(IRole role)
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
			Author = new()
			{
				Name = role.Format(),
			},
			Footer = new()
			{
				Text = GetsFooterRole,
			},
		};
		if (permissions.Length > 0)
		{
			var fieldValue = permissions.Join().WithBlock().Current;
			embed.TryAddField(GetsTitlePermissions, fieldValue, false, out _);
		}
		return Success(embed);
	}

	public static async Task<AdvobotResult> InfoUser(IUser user)
	{
		var sb = new StringBuilder()
			.AppendTimeCreated(user)
			.AppendCategorySeparator()
			.AppendHeaderAndValue(GetsTitleActivity, user.Activities.Select(x => x.Format()).Join("\n"))
			.AppendHeaderAndValue(GetsTitleStatus, user.Status);

		var embed = new EmbedWrapper
		{
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
			Author = new()
			{
				IconUrl = webhook.GetAvatarUrl(),
				Name = webhook.Format(),
				Url = webhook.GetAvatarUrl(ImageFormat.Auto, 2048),
			},
			Footer = new()
			{
				Text = GetsFooterWebhook,
				IconUrl = webhook.GetAvatarUrl(),
			},
		});
	}
}