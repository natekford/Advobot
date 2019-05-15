using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Invites;
using Advobot.Classes.Attributes.Preconditions.Permissions;
using Advobot.Classes.Modules;
using Advobot.Interfaces;
using AdvorangesUtils;
using Discord;
using Discord.Commands;

namespace Advobot.Commands
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
	public abstract class AdvobotGuildListingModule : AdvobotModuleBase
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
	{
		public IInviteListService Invites { get; set; }
	}


	public sealed class GuildList : ModuleBase
	{
		[Group(nameof(ModifyGuildListing)), ModuleInitialismAlias(typeof(ModifyGuildListing))]
		[Summary("Adds or removes a guild from the public guild list.")]
		[UserPermissionRequirement(GuildPermission.Administrator)]
		[EnabledByDefault(false)]
		public sealed class ModifyGuildListing : AdvobotGuildListingModule
		{
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Add([NeverExpires] IInviteMetadata invite, [Optional] params string[] keywords)
			{
				Invites.Add(Context.Guild, invite, keywords);
				return Responses.GuildList.CreatedListing(invite, keywords);
			}
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Remove()
			{
				Invites.Remove(Context.Guild.Id);
				return Responses.GuildList.DeletedListing();
			}
		}

		[Group(nameof(BumpGuildListing)), ModuleInitialismAlias(typeof(BumpGuildListing))]
		[Summary("Bumps the invite on the guild.")]
		[UserPermissionRequirement(PermissionRequirementAttribute.GenericPerms)]
		[EnabledByDefault(false)]
		public sealed class BumpGuildListing : AdvobotGuildListingModule
		{
			[Command]
			public async Task<RuntimeResult> Command()
			{
				if (!(Invites.Get(Context.Guild.Id) is IListedInvite invite))
				{
					return Responses.GuildList.NoInviteToBump();
				}
				if ((DateTime.UtcNow - invite.Time).TotalHours < 1)
				{
					return Responses.GuildList.LastBumpTooRecent();
				}
				await invite.BumpAsync(Context.Guild).CAF();
				return Responses.GuildList.Bumped();
			}
		}

		[Group(nameof(GetGuildListing)), ModuleInitialismAlias(typeof(GetGuildListing))]
		[Summary("Gets an invite meeting the given criteria.")]
		[EnabledByDefault(true)]
		public sealed class GetGuildListing : AdvobotGuildListingModule
		{
			[Command]
			public Task Command([Remainder] ListedInviteGatherer args)
			{
				var invites = args.GatherInvites(Invites).ToArray();
				if (!invites.Any())
				{
					return Responses.GuildList.NoInviteMatch();
				}
				if (invites.Length <= 50)
				{
					return Responses.GuildList.InviteMatches(invites);
				}
				return Responses.GuildList.TooManyMatches();
			}
		}
	}
}
