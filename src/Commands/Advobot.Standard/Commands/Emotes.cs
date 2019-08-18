using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Attributes;
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Emotes;
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles;
using Advobot.Attributes.ParameterPreconditions.Strings;
using Advobot.Attributes.Preconditions;
using Advobot.Attributes.Preconditions.Permissions;
using Advobot.Modules;
using Advobot.Services.ImageResizing;
using Advobot.Standard.Localization;
using Advobot.Standard.Resources;
using AdvorangesUtils;
using Discord;
using Discord.Commands;

namespace Advobot.Standard.Commands
{
	public sealed class Emotes : ModuleBase
	{
		[Group(nameof(CreateEmote)), ModuleInitialismAlias(typeof(CreateEmote))]
		[LocalizedSummary(nameof(Summaries.CreateEmote))]
		[CommandMeta("e001108f-5bae-4589-865e-775a2d21e327", IsEnabled = true)]
		[RateLimit(RateLimitAttribute.TimeUnit.Minutes, 1)]
		[RequireGuildPermissions(GuildPermission.ManageEmojis)]
		public sealed class CreateEmote : ImageResizerModule
		{
			[Command]
			public Task<RuntimeResult> Command(Emote emote)
				=> Command(emote.Name, new Uri(emote.Url));
			[Command, Priority(1)]
			public Task<RuntimeResult> Command(
				[EmoteName] string name,
				Uri url,
				[Optional] UserProvidedImageArgs? args)
			{
				var position = Enqueue(new EmoteCreationContext(Context, url, args, name));
				return Responses.Emotes.EnqueuedCreation(name, position);
			}
		}

		[Group(nameof(DeleteEmote)), ModuleInitialismAlias(typeof(DeleteEmote))]
		[LocalizedSummary(nameof(Summaries.DeleteEmote))]
		[CommandMeta("104da53d-1cb6-4ee4-8260-ac7398512351", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.ManageEmojis)]
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
		[LocalizedSummary(nameof(Summaries.ModifyEmoteName))]
		[CommandMeta("3fe7f72f-c10a-4cc8-b76f-376a1c1aced4", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.ManageEmojis)]
		public sealed class ModifyEmoteName : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				GuildEmote emote,
				[Remainder, EmoteName] string name)
			{
				await Context.Guild.ModifyEmoteAsync(emote, x => x.Name = name, GenerateRequestOptions()).CAF();
				return Responses.Snowflakes.ModifiedName(emote, name);
			}
		}

		[Group(nameof(ModifyEmoteRoles)), ModuleInitialismAlias(typeof(ModifyEmoteRoles))]
		[LocalizedSummary(nameof(Summaries.ModifyEmoteRoles))]
		[CommandMeta("103b0c35-0bd0-4f72-a010-8a4013601258", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.ManageEmojis)]
		public sealed class ModifyEmoteRoles : AdvobotModuleBase
		{
			[ImplicitCommand, ImplicitAlias]
			public async Task<RuntimeResult> Add(
				GuildEmote emote,
				[NotEveryoneOrManaged] params IRole[] roles)
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
				[HasRequiredRoles] GuildEmote emote,
				[NotEveryoneOrManaged] params IRole[] roles)
			{
				await Context.Guild.ModifyEmoteAsync(emote, x => x.Roles = Optional.Create(x.Roles.Value.Where(r => !roles.Contains(r))), GenerateRequestOptions()).CAF();
				return Responses.Emotes.RemoveRequiredRoles(emote, roles);
			}
			[ImplicitCommand, ImplicitAlias]
			public async Task<RuntimeResult> RemoveAll(
				[HasRequiredRoles] GuildEmote emote)
			{
				var roles = emote.RoleIds.Select(x => Context.Guild.GetRole(x));
				await Context.Guild.ModifyEmoteAsync(emote, x => x.Roles = Optional.Create<IEnumerable<IRole>?>(null), GenerateRequestOptions()).CAF();
				return Responses.Emotes.RemoveRequiredRoles(emote, roles);
			}
		}

		[Group(nameof(DisplayEmotes)), ModuleInitialismAlias(typeof(DisplayEmotes))]
		[LocalizedSummary(nameof(Summaries.DisplayEmotes))]
		[CommandMeta("fd5ae4a2-52af-44eb-aef0-347a0df1437b", IsEnabled = true)]
		[RequireGenericGuildPermissions]
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
