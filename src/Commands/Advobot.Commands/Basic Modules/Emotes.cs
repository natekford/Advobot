using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Attributes;
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles;
using Advobot.Attributes.ParameterPreconditions.StringLengthValidation;
using Advobot.Attributes.Preconditions;
using Advobot.Attributes.Preconditions.Permissions;
using Advobot.Classes.Modules;
using Advobot.Services.ImageResizing;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.CommandMarking
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
			public Task<RuntimeResult> Command(Emote emote)
				=> Command(emote.Name, new Uri(emote.Url));
			[Command, Priority(1)]
			public Task<RuntimeResult> Command(
				[ValidateEmoteName] string name,
				Uri url,
				[Optional, Remainder] UserProvidedImageArgs args)
				=> Responses.Emotes.EnqueuedCreation(name, Enqueue(new EmoteCreationContext(Context, url, args, name)));
		}

		[Group(nameof(DeleteEmote)), ModuleInitialismAlias(typeof(DeleteEmote))]
		[Summary("Deletes the supplied emote from the guild.")]
		[UserPermissionRequirement(GuildPermission.ManageEmojis)]
		[EnabledByDefault(true)]
		public sealed class DeleteEmote : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(GuildEmote emote)
			{
				await Context.Guild.DeleteEmoteAsync(emote, GenerateRequestOptions()).CAF();
				return Responses.Snowflakes.Deleted(emote);
			}
		}

		[Group(nameof(ModifyEmoteName)), ModuleInitialismAlias(typeof(ModifyEmoteName))]
		[Summary("Changes the name of the supplied emote.")]
		[UserPermissionRequirement(GuildPermission.ManageEmojis)]
		[EnabledByDefault(true)]
		public sealed class ModifyEmoteName : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				GuildEmote emote,
				[Remainder, ValidateEmoteName] string name)
			{
				await Context.Guild.ModifyEmoteAsync(emote, x => x.Name = name, GenerateRequestOptions()).CAF();
				return Responses.Snowflakes.ModifiedName(emote, name);
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
			public async Task<RuntimeResult> Add(
				GuildEmote emote,
				[NotEveryoneOrManaged] params SocketRole[] roles)
			{
				await Context.Guild.ModifyEmoteAsync(emote, x =>
				{
					var currentRoles = x.Roles.GetValueOrDefault(Enumerable.Empty<IRole>());
					var concat = currentRoles.Concat(roles).Distinct();
					x.Roles = Optional.Create(concat);
				}, GenerateRequestOptions()).CAF();
				return Responses.Emotes.AddedRequiredRoles(emote, roles);
			}
			[ImplicitCommand, ImplicitAlias]
			public async Task<RuntimeResult> Remove(
				GuildEmote emote,
				[NotEveryoneOrManaged] params SocketRole[] roles)
			{
				if (!emote.RoleIds.Any())
				{
					return Responses.Emotes.NoRequiredRoles(emote);
				}

				await Context.Guild.ModifyEmoteAsync(emote, x => x.Roles = Optional.Create(x.Roles.Value.Where(r => !roles.Contains(r))), GenerateRequestOptions()).CAF();
				return Responses.Emotes.RemoveRequiredRoles(emote, roles);
			}
			[ImplicitCommand, ImplicitAlias]
			public async Task<RuntimeResult> RemoveAll(GuildEmote emote)
			{
				if (!emote.RoleIds.Any())
				{
					return Responses.Emotes.NoRequiredRoles(emote);
				}

				var roles = emote.RoleIds.Select(x => Context.Guild.GetRole(x));
				await Context.Guild.ModifyEmoteAsync(emote, x => x.Roles = Optional.Create<IEnumerable<IRole>?>(null), GenerateRequestOptions()).CAF();
				return Responses.Emotes.RemoveRequiredRoles(emote, roles);
			}
		}

		[Group(nameof(DisplayEmotes)), ModuleInitialismAlias(typeof(DisplayEmotes))]
		[Summary("Lists the emotes in the guild. If there are more than 20 emotes of a specified type, they will be uploaded in a file.")]
		[UserPermissionRequirement(PermissionRequirementAttribute.GenericPerms)]
		[EnabledByDefault(true)]
		public sealed class DisplayEmotes : AdvobotModuleBase
		{
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Managed()
				=> Responses.Emotes.DisplayMany(Context.Guild.Emotes.Where(x => x.IsManaged));
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Local()
				=> Responses.Emotes.DisplayMany(Context.Guild.Emotes.Where(x => !x.IsManaged && !x.Animated));
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Animated()
				=> Responses.Emotes.DisplayMany(Context.Guild.Emotes.Where(x => x.Animated));
		}
	}
}
