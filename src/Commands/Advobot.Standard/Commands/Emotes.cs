using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Advobot.Attributes;
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Emotes;
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles;
using Advobot.Attributes.ParameterPreconditions.Strings;
using Advobot.Attributes.Preconditions;
using Advobot.Attributes.Preconditions.Permissions;
using Advobot.Localization;
using Advobot.Modules;
using Advobot.Resources;
using Advobot.Services.ImageResizing;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

namespace Advobot.Standard.Commands
{
	[Category(nameof(Emotes))]
	public sealed class Emotes : ModuleBase
	{
		[LocalizedGroup(nameof(Groups.CreateEmote))]
		[LocalizedAlias(nameof(Aliases.CreateEmote))]
		[LocalizedSummary(nameof(Summaries.CreateEmote))]
		[Meta("e001108f-5bae-4589-865e-775a2d21e327", IsEnabled = true)]
		[RateLimit(RateLimitAttribute.TimeUnit.Minutes, 1)]
		[RequireGuildPermissions(GuildPermission.ManageEmojis)]
		public sealed class CreateEmote : ImageResizerModule
		{
			[Command]
			public Task<RuntimeResult> Command(Emote emote)
				=> Command(emote.Name, new Uri(emote.Url));

			[Command, Priority(1)]
			public Task<RuntimeResult> Command(
				[EmoteName]
				string name,
				Uri url,
				UserProvidedImageArgs? args = null
			)
			{
				args ??= new UserProvidedImageArgs();
				var position = Enqueue(new EmoteCreationContext(Context, url, args, name));
				return Responses.Emotes.EnqueuedCreation(name, position);
			}
		}

		[LocalizedGroup(nameof(Groups.DeleteEmote))]
		[LocalizedAlias(nameof(Aliases.DeleteEmote))]
		[LocalizedSummary(nameof(Summaries.DeleteEmote))]
		[Meta("104da53d-1cb6-4ee4-8260-ac7398512351", IsEnabled = true)]
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

		[LocalizedGroup(nameof(Groups.DisplayEmotes))]
		[LocalizedAlias(nameof(Aliases.DisplayEmotes))]
		[LocalizedSummary(nameof(Summaries.DisplayEmotes))]
		[Meta("fd5ae4a2-52af-44eb-aef0-347a0df1437b", IsEnabled = true)]
		[RequireGenericGuildPermissions]
		public sealed class DisplayEmotes : AdvobotModuleBase
		{
			[LocalizedCommand(nameof(Groups.Animated))]
			[LocalizedAlias(nameof(Aliases.Animated))]
			public Task<RuntimeResult> Animated()
				=> Responses.Emotes.DisplayMany(Context.Guild.Emotes.Where(x => x.Animated));

			[LocalizedCommand(nameof(Groups.Local))]
			[LocalizedAlias(nameof(Aliases.Local))]
			public Task<RuntimeResult> Local()
				=> Responses.Emotes.DisplayMany(Context.Guild.Emotes.Where(x => !x.IsManaged && !x.Animated));

			[LocalizedCommand(nameof(Groups.Managed))]
			[LocalizedAlias(nameof(Aliases.Managed))]
			public Task<RuntimeResult> Managed()
				=> Responses.Emotes.DisplayMany(Context.Guild.Emotes.Where(x => x.IsManaged));
		}

		[LocalizedGroup(nameof(Groups.ModifyEmoteName))]
		[LocalizedAlias(nameof(Aliases.ModifyEmoteName))]
		[LocalizedSummary(nameof(Summaries.ModifyEmoteName))]
		[Meta("3fe7f72f-c10a-4cc8-b76f-376a1c1aced4", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.ManageEmojis)]
		public sealed class ModifyEmoteName : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				GuildEmote emote,
				[Remainder, EmoteName]
				string name
			)
			{
				await Context.Guild.ModifyEmoteAsync(emote, x => x.Name = name, GenerateRequestOptions()).CAF();
				return Responses.Snowflakes.ModifiedName(emote, name);
			}
		}

		[LocalizedGroup(nameof(Groups.ModifyEmoteRoles))]
		[LocalizedAlias(nameof(Aliases.ModifyEmoteRoles))]
		[LocalizedSummary(nameof(Summaries.ModifyEmoteRoles))]
		[Meta("103b0c35-0bd0-4f72-a010-8a4013601258", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.ManageEmojis)]
		public sealed class ModifyEmoteRoles : AdvobotModuleBase
		{
			[LocalizedCommand(nameof(Groups.Add))]
			[LocalizedAlias(nameof(Aliases.Add))]
			public async Task<RuntimeResult> Add(
				GuildEmote emote,
				[NotEveryone, NotManaged]
				params IRole[] roles
			)
			{
				await Context.Guild.ModifyEmoteAsync(emote, x =>
				{
					var currentRoles = x.Roles.GetValueOrDefault(Enumerable.Empty<IRole>());
					var concat = currentRoles.Concat(roles).Distinct();
					x.Roles = Optional.Create(concat);
				}, GenerateRequestOptions()).CAF();
				return Responses.Emotes.AddedRequiredRoles(emote, roles);
			}

			[LocalizedCommand(nameof(Groups.Remove))]
			[LocalizedAlias(nameof(Aliases.Remove))]
			public async Task<RuntimeResult> Remove(
				[HasRequiredRoles]
				GuildEmote emote,
				[NotEveryone, NotManaged]
				params IRole[] roles
			)
			{
				await Context.Guild.ModifyEmoteAsync(emote, x => x.Roles = Optional.Create(x.Roles.Value.Where(r => !roles.Contains(r))), GenerateRequestOptions()).CAF();
				return Responses.Emotes.RemoveRequiredRoles(emote, roles);
			}

			[LocalizedCommand(nameof(Groups.RemoveAll))]
			[LocalizedAlias(nameof(Aliases.RemoveAll))]
			public async Task<RuntimeResult> RemoveAll(
				[HasRequiredRoles]
				GuildEmote emote
			)
			{
				var roles = emote.RoleIds.Select(x => Context.Guild.GetRole(x));
				await Context.Guild.ModifyEmoteAsync(emote, x => x.Roles = Optional.Create<IEnumerable<IRole>?>(null), GenerateRequestOptions()).CAF();
				return Responses.Emotes.RemoveRequiredRoles(emote, roles);
			}
		}
	}
}