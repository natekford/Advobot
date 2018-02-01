using Advobot.Core.Classes;
using Advobot.Core.Classes.Attributes;
using Advobot.Core.Enums;
using Advobot.Core.Utilities;
using Advobot.Core.Utilities.Formatting;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Advobot.Commands.Gets
{
	[Group(nameof(GetId)), TopLevelShortAlias(typeof(GetId))]
	[Summary("Shows the ID of the given object. " +
	"Channels, roles, users, and emojis need to be supplied for the command to work if targetting those.")]
	[DefaultEnabled(true)]
	public sealed class GetId : NonSavingModuleBase
	{
		[Command(nameof(Bot)), ShortAlias(nameof(Bot))]
		public async Task Bot()
		{
			await MessageUtils.SendMessageAsync(Context.Channel, $"The bot has the ID `{Context.Client.CurrentUser.Id}`.").CAF();
		}
		[Command(nameof(Guild)), ShortAlias(nameof(Guild))]
		public async Task Guild()
		{
			await MessageUtils.SendMessageAsync(Context.Channel, $"The guild has the ID `{Context.Guild.Id}`.").CAF();
		}
		[Command(nameof(Channel)), ShortAlias(nameof(Channel))]
		public async Task Channel(IGuildChannel channel)
		{
			await MessageUtils.SendMessageAsync(Context.Channel, $"The channel `{channel.Name}` has the ID `{channel.Id}`.").CAF();
		}
		[Command(nameof(Role)), ShortAlias(nameof(Role))]
		public async Task Role(IRole role)
		{
			await MessageUtils.SendMessageAsync(Context.Channel, $"The role `{role.Name}` has the ID `{role.Id}`.").CAF();
		}
		[Command(nameof(User)), ShortAlias(nameof(User))]
		public async Task User(IUser user)
		{
			await MessageUtils.SendMessageAsync(Context.Channel, $"The user `{user.Username}` has the ID `{user.Id}`.").CAF();
		}
		[Command(nameof(Emote)), ShortAlias(nameof(Emote))]
		public async Task Emote(Emote emote)
		{
			await MessageUtils.SendMessageAsync(Context.Channel, $"The emote `{emote.Name}` has the ID `{emote.Id}`.").CAF();
		}
	}

	[Group(nameof(GetInfo)), TopLevelShortAlias(typeof(GetInfo))]
	[Summary("Shows information about the given object. " +
		"Channels, roles, users, and emojis need to be supplied for the command to work if targetting those.")]
	[DefaultEnabled(true)]
	public sealed class GetInfo : NonSavingModuleBase
	{
		[Command(nameof(Bot)), ShortAlias(nameof(Bot))]
		public async Task Bot()
		{
			var embed = InfoFormatting.FormatBotInfo(Context.Client, Context.Logging, Context.Guild);
			await MessageUtils.SendEmbedMessageAsync(Context.Channel, embed).CAF();
		}
		[Command(nameof(Guild)), ShortAlias(nameof(Guild))]
		public async Task Guild()
		{
			var embed = InfoFormatting.FormatGuildInfo(Context.Guild as SocketGuild);
			await MessageUtils.SendEmbedMessageAsync(Context.Channel, embed).CAF();
		}
		[Command(nameof(Channel)), ShortAlias(nameof(Channel))]
		public async Task Channel(IGuildChannel channel)
		{
			var embed = InfoFormatting.FormatChannelInfo(Context.GuildSettings, channel as SocketChannel);
			await MessageUtils.SendEmbedMessageAsync(Context.Channel, embed).CAF();
		}
		[Command(nameof(Role)), ShortAlias(nameof(Role))]
		public async Task Role(IRole role)
		{
			var embed = InfoFormatting.FormatRoleInfo(Context.Guild as SocketGuild, role as SocketRole);
			await MessageUtils.SendEmbedMessageAsync(Context.Channel, embed).CAF();
		}
		[Command(nameof(User)), ShortAlias(nameof(User))]
		public async Task User(IUser user)
		{
			if (user is SocketGuildUser socketGuildUser)
			{
				var embed = InfoFormatting.FormatUserInfo(Context.Guild as SocketGuild, socketGuildUser);
				await MessageUtils.SendEmbedMessageAsync(Context.Channel, embed).CAF();
			}
			else if (user is SocketUser socketUser)
			{
				var embed = InfoFormatting.FormatUserInfo(socketUser);
				await MessageUtils.SendEmbedMessageAsync(Context.Channel, embed).CAF();
			}
		}
		[Command(nameof(Emote)), ShortAlias(nameof(Emote))]
		public async Task Emote(Emote emote)
		{
			var embed = InfoFormatting.FormatEmoteInfo(emote);
			await MessageUtils.SendEmbedMessageAsync(Context.Channel, embed).CAF();
		}
		[Command(nameof(Invite)), ShortAlias(nameof(Invite))]
		public async Task Invite(IInvite invite)
		{
			var embed = InfoFormatting.FormatInviteInfo(invite as IInviteMetadata);
			await MessageUtils.SendEmbedMessageAsync(Context.Channel, embed).CAF();
		}
	}

	[Group(nameof(GetUsersWithReason)), TopLevelShortAlias(typeof(GetUsersWithReason))]
	[Summary("Gets users with a variable reason. " +
		"`Count` specifies if to say the count. " +
		"`Nickname` specifies if to include nickanmes. " +
		"`Exact` specifies if only exact matches apply.")]
	[OtherRequirement(Precondition.GenericPerms)]
	[DefaultEnabled(true)]
	public sealed class GetUsersWithReason : NonSavingModuleBase
	{
		[Command(nameof(Role)), ShortAlias(nameof(Role))]
		public async Task Role(IRole role, params SearchOptions[] additionalSearchOptions)
		{
			await CommandRunner(Target.Role, role, additionalSearchOptions).CAF();
		}
		[Command(nameof(Name)), ShortAlias(nameof(Name))]
		public async Task Name(string name, params SearchOptions[] additionalSearchOptions)
		{
			await CommandRunner(Target.Name, name, additionalSearchOptions).CAF();
		}
		[Command(nameof(Game)), ShortAlias(nameof(Game))]
		public async Task Game(string game, params SearchOptions[] additionalSearchOptions)
		{
			await CommandRunner(Target.Game, game, additionalSearchOptions).CAF();
		}
		[Command(nameof(Stream)), ShortAlias(nameof(Stream))]
		public async Task Stream(params SearchOptions[] additionalSearchOptions)
		{
			await CommandRunner(Target.Stream, null, additionalSearchOptions).CAF();
		}

		private async Task CommandRunner(Target targetType, object obj, SearchOptions[] additionalSearchOptions)
		{
			var count = additionalSearchOptions.Contains(SearchOptions.Count);
			var nickname = additionalSearchOptions.Contains(SearchOptions.Nickname);
			var exact = additionalSearchOptions.Contains(SearchOptions.Exact);

			string title;
			var users = (await Context.Guild.GetUsersAsync().CAF()).AsEnumerable();
			switch (targetType)
			{
				case Target.Role:
					var role = obj as IRole;
					title = $"Users With The Role '{role?.Name}'";
					users = users.Where(x => x.RoleIds.Contains(role?.Id ?? 0));
					break;
				case Target.Name:
					var name = obj.ToString();
					title = $"Users With Names Containing '{obj}'";
					users = users.Where(x => exact ? x.Username.CaseInsEquals(name) || (nickname && x.Nickname.CaseInsEquals(name))
												   : x.Username.CaseInsContains(name) || (nickname && x.Nickname.CaseInsContains(name)));
					break;
				case Target.Game:
					var game = obj.ToString();
					title = $"Users With Games Containing '{obj}'";
					users = users.Where(x =>
					{
						if (!(x.Activity is Game g))
						{
							return false;
						}
						return exact ? g.Name.CaseInsEquals(game) : g.Name.CaseInsContains(game);
					});
					break;
				case Target.Stream:
					title = "Users Who Are Streaming";
					users = users.Where(x => x.Activity is StreamingGame);
					break;
				default:
					return;
			}

			var desc = count
				? $"**Count:** `{users.Count()}`"
				: users.OrderBy(x => x.JoinedAt).FormatNumberedList(x => x.Format());
			var embed = new EmbedWrapper
			{
				Title = title,
				Description = desc
			};
			await MessageUtils.SendEmbedMessageAsync(Context.Channel, embed).CAF();
		}

		public enum SearchOptions
		{
			Count,
			Nickname,
			Exact
		}
	}

	[Group(nameof(GetUserAvatar)), TopLevelShortAlias(typeof(GetUserAvatar))]
	[Summary("Shows the URL of the given user's avatar. Must supply a format, can supply a size, and can specify which user.")]
	[DefaultEnabled(true)]
	public sealed class GetUserAvatar : NonSavingModuleBase
	{
		[Command, Priority(0)]
		public async Task Command(ImageFormat format, [Optional] IUser user, [Optional] ushort size)
		{
			await Context.Channel.SendMessageAsync((user ?? Context.User).GetAvatarUrl(format, size == 0 ? (ushort)128 : size)).CAF();
		}
		[Command, Priority(1)]
		public async Task Command(ImageFormat format, [Optional] ushort size, [Optional] IUser user)
		{
			await Context.Channel.SendMessageAsync((user ?? Context.User).GetAvatarUrl(format, size == 0 ? (ushort)128 : size)).CAF();
		}
	}

	[Group(nameof(GetUserJoinedAt)), TopLevelShortAlias(typeof(GetUserJoinedAt))]
	[Summary("Shows the user which joined the guild in that position.")]
	[OtherRequirement(Precondition.GenericPerms)]
	[DefaultEnabled(true)]
	public sealed class GetUserJoinedAt : NonSavingModuleBase
	{
		[Command]
		public async Task Command(uint position)
		{
			var users = (await Context.Guild.GetUsersByJoinDateAsync().CAF()).ToArray();
			var newPos = Math.Max(1, Math.Min((int)position, users.Length));
			var user = users[newPos - 1];
			var time = user.JoinedAt?.UtcDateTime.Readable();
			var text = $"`{user.Format()}` is `#{newPos}` to join the guild on `{time}`.";
			await MessageUtils.SendMessageAsync(Context.Channel, text).CAF();
		}
	}

	[Group(nameof(GetGuilds)), TopLevelShortAlias(typeof(GetGuilds))]
	[Summary("Lists the name, id, and owner of every guild the bot is on.")]
	[OtherRequirement(Precondition.BotOwner)]
	[DefaultEnabled(true)]
	public sealed class GetGuilds : NonSavingModuleBase
	{
		[Command(RunMode = RunMode.Async)]
		public async Task Command()
		{
			var guilds = await Context.Client.GetGuildsAsync().CAF();
			if (guilds.Count() <= 10)
			{
				var embed = new EmbedWrapper
				{
					Title = "Guilds"
				};
				foreach (var guild in guilds)
				{
					embed.TryAddField(guild.Format(), $"**Owner:** `{(await guild.GetOwnerAsync().CAF()).Format()}`", false, out _);
				}

				await MessageUtils.SendEmbedMessageAsync(Context.Channel, embed).CAF();
			}
			else
			{
				var guildsAndOwners = await Task.WhenAll(guilds.Select(async x => (Guild: x, Owner: await x.GetOwnerAsync().CAF())));
				var desc = guildsAndOwners.FormatNumberedList(x => $"`{x.Guild.Format()}` Owner: `{x.Owner.Format()}`");
				var embed = new EmbedWrapper
				{
					Title = "Guilds",
					Description = desc
				};
				await MessageUtils.SendEmbedMessageAsync(Context.Channel, embed).CAF();
			}
		}
	}

	[Group(nameof(GetUserJoinList)), TopLevelShortAlias(typeof(GetUserJoinList))]
	[Summary("Lists most of the users who have joined the guild.")]
	[OtherRequirement(Precondition.GenericPerms)]
	[DefaultEnabled(true)]
	public sealed class GetUserJoinList : NonSavingModuleBase
	{
		[Command(RunMode = RunMode.Async)]
		public async Task Command()
		{
			var users = (await Context.Guild.GetUsersByJoinDateAsync().CAF()).ToArray();
			var text = users.FormatNumberedList(x => $"`{x.Format()}` joined on `{x.JoinedAt?.UtcDateTime.Readable()}`");
			await MessageUtils.SendTextFileAsync(Context.Channel, text, "User_Joins_").CAF();
		}
	}

	[Group(nameof(GetEmotes)), TopLevelShortAlias(typeof(GetEmotes))]
	[Summary("Lists the emotes in the guild.")]
	[OtherRequirement(Precondition.GenericPerms)]
	[DefaultEnabled(true)]
	public sealed class GetEmotes : NonSavingModuleBase
	{
		[Command(nameof(Managed)), ShortAlias(nameof(Managed))]
		public async Task Managed()
		{
			var emotes = Context.Guild.Emotes.Where(x => x.IsManaged).ToList();
			var desc = emotes.Any()
				? emotes.FormatNumberedList(x => $"<:{x.Name}:{x.Id}> `{x.Name}`")
				: $"This guild has no global emotes.";
			var embed = new EmbedWrapper
			{
				Title = "Emotes",
				Description = desc
			};
			await MessageUtils.SendEmbedMessageAsync(Context.Channel, embed).CAF();
		}
		[Command(nameof(Guild)), ShortAlias(nameof(Guild))]
		public async Task Guild()
		{
			var emotes = Context.Guild.Emotes.Where(x => !x.IsManaged).ToList();
			var desc = emotes.Any()
				? emotes.FormatNumberedList(x => $"<:{x.Name}:{x.Id}> `{x.Name}`")
				: $"This guild has no guild emotes.";
			var embed = new EmbedWrapper
			{
				Title = "Emotes",
				Description = desc
			};
			await MessageUtils.SendEmbedMessageAsync(Context.Channel, embed).CAF();
		}
	}

	[Group(nameof(GetMessages)), TopLevelShortAlias(typeof(GetMessages))]
	[Summary("Downloads the past x amount of messages. " +
		"Up to 1000 messages or 500KB worth of formatted text.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(true)]
	public sealed class GetMessages : NonSavingModuleBase
	{
		[Command(RunMode = RunMode.Async)]
		public async Task Command(int number, [Optional, VerifyObject(true, ObjectVerification.CanBeRead)] ITextChannel channel)
		{
			channel = channel ?? Context.Channel as ITextChannel;
			var messages = await MessageUtils.GetMessagesAsync(channel, Math.Min(number, 1000)).CAF();
			var m = messages.OrderBy(x => x.CreatedAt.Ticks).ToArray();

			var formattedMessagesBuilder = new StringBuilder();
			for (int count = 0; count < m.Length; ++count)
			{
				var text = m[count].Format(false).RemoveAllMarkdown().RemoveDuplicateNewLines();
				if (formattedMessagesBuilder.Length + text.Length < Context.BotSettings.MaxMessageGatherSize)
				{
					formattedMessagesBuilder.AppendLineFeed(text);
				}
				else
				{
					var fileName = $"{channel?.Name}_Messages";
					var content = $"Successfully got `{count}` messages";
					await MessageUtils.SendTextFileAsync(Context.Channel, formattedMessagesBuilder.ToString(), fileName, content).CAF();
					return;
				}
			}
		}
	}

	[Group(nameof(GetPermNamesFromValue)), TopLevelShortAlias(typeof(GetPermNamesFromValue))]
	[Summary("Lists all the perms that come from the given value.")]
	[OtherRequirement(Precondition.GenericPerms)]
	[DefaultEnabled(true)]
	public sealed class GetPermNamesFromValue : NonSavingModuleBase
	{
		[Command(nameof(Guild)), ShortAlias(nameof(Guild))]
		public async Task Guild(ulong number)
		{
			var perms = EnumUtils.GetFlagNames((GuildPermission)number);
			if (!perms.Any())
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("The given number holds no permissions.")).CAF();
			}
			else
			{
				var resp = $"The number `{number}` has the following guild permissions: `{String.Join("`, `", perms)}`.";
				await MessageUtils.SendMessageAsync(Context.Channel, resp).CAF();
			}
		}
		[Command(nameof(Channel)), ShortAlias(nameof(Channel))]
		public async Task Channel(ulong number)
		{
			var perms = EnumUtils.GetFlagNames((ChannelPermission)number);
			if (!perms.Any())
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("The given number holds no permissions.")).CAF();
			}
			else
			{
				var resp = $"The number `{number}` has the following channel permissions: `{String.Join("`, `", perms)}`.";
				await MessageUtils.SendMessageAsync(Context.Channel, resp).CAF();
			}
		}
	}

	[Group(nameof(GetEnumNames)), TopLevelShortAlias(typeof(GetEnumNames))]
	[Summary("Prints out all the options of an enum.")]
	[OtherRequirement(Precondition.GenericPerms)]
	[DefaultEnabled(true)]
	public sealed class GetEnumNames : NonSavingModuleBase
	{
		private static ImmutableArray<Type> _Enums = AppDomain.CurrentDomain.GetAssemblies()
			.Where(x => x.FullName.CaseInsContains("discord") || x.FullName.CaseInsContains("advobot"))
			.SelectMany(x => x.GetTypes()).Where(x => x.IsEnum && x.IsPublic)
			.Distinct().OrderBy(x => x.Name)
			.ToImmutableArray();

		[Command(nameof(Show)), ShortAlias(nameof(Show)), Priority(1)]
		public async Task Show()
		{
			var embed = new EmbedWrapper
			{
				Title = "Enums",
				Description = $"`{String.Join("`, `", _Enums.Select(x => x.Name))}`"
			};
			await MessageUtils.SendEmbedMessageAsync(Context.Channel, embed).CAF();
		}
		[Command]
		public async Task Command(string enumName)
		{
			var matchingNames = _Enums.Where(x => x.Name.CaseInsEquals(enumName)).ToList();
			if (!matchingNames.Any())
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error($"No enum has the name `{enumName}`.")).CAF();
				return;
			}
			if (matchingNames.Count() > 1)
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error($"Too many enums have the name `{enumName}`.")).CAF();
				return;
			}

			var e = matchingNames.Single();
			var embed = new EmbedWrapper
			{
				Title = e.Name,
				Description = $"`{String.Join("`, `", Enum.GetNames(e))}`"
			};
			await MessageUtils.SendEmbedMessageAsync(Context.Channel, embed).CAF();
		}
	}
}
