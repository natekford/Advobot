using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Classes.Attributes;
using Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles;
using Advobot.Classes.Attributes.ParameterPreconditions.StringValidation;
using Advobot.Classes.Attributes.Preconditions;
using Advobot.Classes.Attributes.Preconditions.Permissions;
using Advobot.Classes.ImageResizing;
using Advobot.Classes.Modules;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Commands
{
	public sealed class Emotes : ModuleBase
	{
		[Group(nameof(CreateEmote)), ModuleInitialismAlias(typeof(CreateEmote))]
		[Summary("Adds an emote to the server. " +
			"Requires either an emote to copy, or the name and file to make an emote out of.")]
		[UserPermissionRequirement(GuildPermission.ManageEmojis)]
		[EnabledByDefault(true)]
		[RateLimit(RateLimitAttribute.TimeUnit.Minutes, 1)]
		public sealed class CreateEmote : ImageResizerModule
		{
			[Command]
			public Task Command(Emote emote)
				=> Command(emote.Name, new Uri(emote.Url));
			[Command, Priority(1)]
			public Task Command(
				[ValidateEmoteName] string name,
				Uri url,
				[Optional, Remainder] UserProvidedImageArgs args)
				=> ProcessAsync(new EmoteCreationArgs(Context, url, args, name));
		}

		[Group(nameof(DeleteEmote)), ModuleInitialismAlias(typeof(DeleteEmote))]
		[Summary("Deletes the supplied emote from the guild.")]
		[UserPermissionRequirement(GuildPermission.ManageEmojis)]
		[EnabledByDefault(true)]
		public sealed class DeleteEmote : AdvobotModuleBase
		{
			[Command]
			public async Task Command(GuildEmote emote)
			{
				await Context.Guild.DeleteEmoteAsync(emote, GenerateRequestOptions()).CAF();
				await ReplyTimedAsync($"Successfully deleted the emote `{emote.Name}`.").CAF();
			}
		}

		[Group(nameof(ModifyEmoteName)), ModuleInitialismAlias(typeof(ModifyEmoteName))]
		[Summary("Changes the name of the supplied emote.")]
		[UserPermissionRequirement(GuildPermission.ManageEmojis)]
		[EnabledByDefault(true)]
		public sealed class ModifyEmoteName : AdvobotModuleBase
		{
			[Command]
			public async Task Command(GuildEmote emote, [Remainder, ValidateEmoteName] string name)
			{
				await Context.Guild.ModifyEmoteAsync(emote, x => x.Name = name, GenerateRequestOptions()).CAF();
				await ReplyTimedAsync($"Successfully changed the emote name to `{name}`.").CAF();
			}
		}

		[Group(nameof(ModifyEmoteRoles)), ModuleInitialismAlias(typeof(ModifyEmoteRoles))]
		[Summary("Changes the roles which are ALL necessary to use an emote. " +
			"Your Discord client will need to be restarted after editing this in order to see the emote again, even if you give yourself the roles.")]
		[UserPermissionRequirement(GuildPermission.ManageEmojis)]
		[EnabledByDefault(true)]
		public sealed class ModifyEmoteRoles : AdvobotModuleBase
		{
			[ImplicitCommand, ImplicitAlias]
			public async Task Add(GuildEmote emote, [NotEveryoneOrManaged] params SocketRole[] roles)
			{
				await Context.Guild.ModifyEmoteAsync(emote, x =>
				{
					var currentRoles = x.Roles.GetValueOrDefault() ?? Enumerable.Empty<IRole>();
					var concat = currentRoles.Concat(roles).Distinct();
					x.Roles = Optional.Create(concat);
				}, GenerateRequestOptions()).CAF();
				await ReplyTimedAsync($"Successfully added `{roles.Join("`, `", x => x.Format())}` as roles necessary to use `{emote}`.").CAF();
			}
			[ImplicitCommand, ImplicitAlias]
			public async Task Remove(GuildEmote emote, [NotEveryoneOrManaged] params SocketRole[] roles)
			{
				if (!emote.RoleIds.Any())
				{
					await ReplyErrorAsync($"The emote `{emote}` does not have any restricting roles.").CAF();
					return;
				}

				await Context.Guild.ModifyEmoteAsync(emote, x =>
				{
					if (!x.Roles.IsSpecified)
					{
						return;
					}

					var ids = roles.Select(r => r.Id);
					x.Roles = Optional.Create(x.Roles.Value.Where(r => !ids.Contains(r.Id)));
				}, GenerateRequestOptions()).CAF();
				await ReplyTimedAsync($"Successfully removed `{roles.Join("`, `", x => x.Format())}` as roles necessary to use `{emote}`.").CAF();
			}
			[ImplicitCommand, ImplicitAlias]
			public async Task RemoveAll(GuildEmote emote)
			{
				if (!emote.RoleIds.Any())
				{
					await ReplyErrorAsync($"The emote `{emote}` does not have any restricting roles.").CAF();
					return;
				}

				await Context.Guild.ModifyEmoteAsync(emote, x => x.Roles = Optional.Create<IEnumerable<IRole>>(null), GenerateRequestOptions()).CAF();
				await ReplyTimedAsync($"Successfully removed all roles necessary to use `{emote}`.").CAF();
			}
		}

		[Group(nameof(DisplayEmotes)), ModuleInitialismAlias(typeof(DisplayEmotes))]
		[Summary("Lists the emotes in the guild. If there are more than 20 emotes of a specified type, they will be uploaded in a file.")]
		[UserPermissionRequirement(PermissionRequirementAttribute.GenericPerms)]
		[EnabledByDefault(true)]
		public sealed class DisplayEmotes : AdvobotModuleBase
		{
			[ImplicitCommand, ImplicitAlias]
			public Task Managed()
				=> CommandRunner(x => x.IsManaged);
			[ImplicitCommand, ImplicitAlias]
			public Task Local()
				=> CommandRunner(x => !x.IsManaged && !x.Animated);
			[ImplicitCommand, ImplicitAlias]
			public Task Animated()
				=> CommandRunner(x => x.Animated);

			private Task CommandRunner(Func<GuildEmote, bool> predicate, [CallerMemberName] string caller = "")
				=> ReplyIfAny(Context.Guild.Emotes.Where(predicate), caller + " Emotes", x => $"{x} `{x.Name}`");
		}
	}
}
