using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Enums;
using Advobot.Utilities;
using AdvorangesUtils;
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
	[Group(nameof(GetInfo)), TopLevelShortAlias(typeof(GetInfo))]
	[Summary("Shows information about the given object. " +
		"Channels, roles, users, and emojis need to be supplied for the command to work if targetting those.")]
	[DefaultEnabled(true)]
	public sealed class GetInfo : NonSavingModuleBase
	{
		[Command(nameof(Bot)), ShortAlias(nameof(Bot))]
		public async Task Bot()
		{
			var embed = DiscordFormatting.FormatBotInfo(Context.Client, Context.Logging, Context.Guild);
			await MessageUtils.SendMessageAsync(Context.Channel, null, embed).CAF();
		}
		[Command(nameof(Guild)), ShortAlias(nameof(Guild))]
		public async Task Guild()
		{
			var embed = DiscordFormatting.FormatGuildInfo(Context.Guild);
			await MessageUtils.SendMessageAsync(Context.Channel, null, embed).CAF();
		}
		[Command(nameof(GuildUsers)), ShortAlias(nameof(GuildUsers))]
		public async Task GuildUsers()
		{
			var embed = DiscordFormatting.FormatAllGuildUsersInfo(Context.Guild);
			await MessageUtils.SendMessageAsync(Context.Channel, null, embed).CAF();
		}
		[Command(nameof(Channel)), ShortAlias(nameof(Channel))]
		public async Task Channel([Remainder] SocketGuildChannel channel)
		{
			var embed = DiscordFormatting.FormatChannelInfo(Context.GuildSettings, channel);
			await MessageUtils.SendMessageAsync(Context.Channel, null, embed).CAF();
		}
		[Command(nameof(Role)), ShortAlias(nameof(Role))]
		public async Task Role([Remainder] SocketRole role)
		{
			var embed = DiscordFormatting.FormatRoleInfo(role);
			await MessageUtils.SendMessageAsync(Context.Channel, null, embed).CAF();
		}
		[Command(nameof(User)), ShortAlias(nameof(User))]
		public async Task User([Remainder] SocketUser user)
		{
			var embed = user is SocketGuildUser guildUser
				? DiscordFormatting.FormatGuildUserInfo(guildUser)
				: DiscordFormatting.FormatUserInfo(user);
			await MessageUtils.SendMessageAsync(Context.Channel, null, embed).CAF();
		}
		[Command(nameof(Emote)), ShortAlias(nameof(Emote))]
		public async Task Emote([Remainder] Emote emote)
		{
			var embed = emote is GuildEmote guildEmote
				? DiscordFormatting.FormatGuildEmoteInfo(Context.Guild, guildEmote)
				: DiscordFormatting.FormatEmoteInfo(emote);
			await MessageUtils.SendMessageAsync(Context.Channel, null, embed).CAF();
		}
		[Command(nameof(Invite)), ShortAlias(nameof(Invite))]
		public async Task Invite([Remainder] IInvite invite)
		{
			var embed = DiscordFormatting.FormatInviteInfo(invite as IInviteMetadata);
			await MessageUtils.SendMessageAsync(Context.Channel, null, embed).CAF();
		}
		[Command(nameof(Webhook)), ShortAlias(nameof(Webhook))]
		public async Task Webhook([Remainder] IWebhook webhook)
		{
			var embed = DiscordFormatting.FormatWebhookInfo(Context.Guild, webhook);
			await MessageUtils.SendMessageAsync(Context.Channel, null, embed).CAF();
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
		public async Task Role(SocketRole role, params SearchOptions[] additionalSearchOptions)
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
			var users = Context.Guild.Users.AsEnumerable();
			switch (targetType)
			{
				case Target.Role:
					var role = obj as IRole;
					title = $"Users With The Role '{role?.Name}'";
					users = users.Where(u => u.Roles.Select(r => r.Id).Contains(role?.Id ?? 0));
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
			await MessageUtils.SendMessageAsync(Context.Channel, null, embed).CAF();
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
		[Command]
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
			var users = Context.Guild.GetUsersByJoinDate().ToArray();
			var newPos = Math.Min((int)position, users.Length);
			var user = users[newPos - 1];
			var text = $"`{user.Format()}` is `#{newPos}` to join the guild on `{user.JoinedAt?.UtcDateTime.ToReadable()}`.";
			await MessageUtils.SendMessageAsync(Context.Channel, text).CAF();
		}
	}

	[Group(nameof(GetGuilds)), TopLevelShortAlias(typeof(GetGuilds))]
	[Summary("Lists the name, id, and owner of every guild the bot is on.")]
	[OtherRequirement(Precondition.BotOwner)]
	[DefaultEnabled(true)]
	public sealed class GetGuilds : NonSavingModuleBase
	{
		[Command]
		public async Task Command()
		{
			var guilds = Context.Client.Guilds;
			if (guilds.Count() <= EmbedBuilder.MaxFieldCount)
			{
				var embed = new EmbedWrapper
				{
					Title = "Guilds",
				};
				foreach (var guild in guilds)
				{
					embed.TryAddField(guild.Format(), $"**Owner:** `{guild.Owner.Format()}`", false, out _);
				}
				await MessageUtils.SendMessageAsync(Context.Channel, null, embed).CAF();
			}
			else
			{
				var embed = new EmbedWrapper
				{
					Title = "Guilds",
					Description = guilds.FormatNumberedList(x => $"`{x.Format()}` Owner: `{x.Owner.Format()}`"),
				};
				await MessageUtils.SendMessageAsync(Context.Channel, null, embed).CAF();
			}
		}
	}

	[Group(nameof(GetUserJoinList)), TopLevelShortAlias(typeof(GetUserJoinList))]
	[Summary("Lists most of the users who have joined the guild.")]
	[OtherRequirement(Precondition.GenericPerms)]
	[DefaultEnabled(true)]
	public sealed class GetUserJoinList : NonSavingModuleBase
	{
		[Command]
		public async Task Command()
		{
			var users = Context.Guild.GetUsersByJoinDate().ToArray();
			var text = users.FormatNumberedList(x => $"`{x.Format()}` joined on `{x.JoinedAt?.UtcDateTime.ToReadable()}`");
			var tf = new TextFileInfo
			{
				Name = "User_Joins",
				Text = text,
			};
			await MessageUtils.SendMessageAsync(Context.Channel, $"**User Join List:**", textFile: tf).CAF();
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
		public async Task Command(int number, [Optional, VerifyObject(true, ObjectVerification.CanBeViewed)] SocketTextChannel channel)
		{
			channel = channel ?? (SocketTextChannel)Context.Channel;
			var messages = await MessageUtils.GetMessagesAsync(channel, Math.Min(number, 1000)).CAF();
			var m = messages.OrderBy(x => x.CreatedAt.Ticks).ToArray();

			var formattedMessagesBuilder = new StringBuilder();
			for (int count = 0; count < m.Length; ++count)
			{
				var text = m[count].Format(withMentions: false).RemoveAllMarkdown().RemoveDuplicateNewLines();
				if (formattedMessagesBuilder.Length + text.Length < Context.BotSettings.MaxMessageGatherSize)
				{
					formattedMessagesBuilder.AppendLineFeed(text);
				}
				else
				{
					var tf = new TextFileInfo
					{
						Name = $"{channel?.Name}_Messages",
						Text = formattedMessagesBuilder.ToString()
					};
					await MessageUtils.SendMessageAsync(Context.Channel, $"**{count} Messages:**", textFile: tf).CAF();
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
				return;
			}
			var resp = $"The number `{number}` has the following guild permissions: `{String.Join("`, `", perms)}`.";
			await MessageUtils.SendMessageAsync(Context.Channel, resp).CAF();
		}
		[Command(nameof(Channel)), ShortAlias(nameof(Channel))]
		public async Task Channel(ulong number)
		{
			var perms = EnumUtils.GetFlagNames((ChannelPermission)number);
			if (!perms.Any())
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("The given number holds no permissions.")).CAF();
				return;
			}
			var resp = $"The number `{number}` has the following channel permissions: `{String.Join("`, `", perms)}`.";
			await MessageUtils.SendMessageAsync(Context.Channel, resp).CAF();
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
			await MessageUtils.SendMessageAsync(Context.Channel, null, embed).CAF();
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
			await MessageUtils.SendMessageAsync(Context.Channel, null, embed).CAF();
		}
	}
}
