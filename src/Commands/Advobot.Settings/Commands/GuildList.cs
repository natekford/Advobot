using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Advobot.Attributes;
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Invites;
using Advobot.Attributes.Preconditions;
using Advobot.Attributes.Preconditions.Permissions;
using Advobot.Classes;
using Advobot.Modules;
using Advobot.Services.InviteList;
using Advobot.Settings.Localization;
using Advobot.Settings.Resources;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

namespace Advobot.Settings.Commands
{
	[Category(nameof(GuildList))]
	public sealed class GuildList : ModuleBase
	{
		[Group(nameof(ModifyGuildListing)), ModuleInitialismAlias(typeof(ModifyGuildListing))]
		[LocalizedSummary(nameof(Summaries.ModifyGuildListing))]
		[Meta("ad81af3b-c2d7-4e49-9cef-2be7f0c6cf9e")]
		[RequireGuildPermissions]
		public sealed class ModifyGuildListing : AdvobotModuleBase
		{
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
			public IInviteListService Invites { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.

			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Add(
				[NeverExpires, FromThisGuild] IInviteMetadata invite,
				[Optional] params string[] keywords)
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
		[LocalizedSummary(nameof(Summaries.BumpGuildListing))]
		[Meta("7522e03e-a53a-4ac6-b522-54db5b7b0d05")]
		[RequireGenericGuildPermissions]
		[RequireNotRecentlyBumped]
		public sealed class BumpGuildListing : AdvobotModuleBase
		{
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
			public IInviteListService Invites { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.

			[Command]
			public async Task<RuntimeResult> Command()
			{
				var invite = Invites.Get(Context.Guild.Id);
				await invite.BumpAsync(Context.Guild).CAF();
				return Responses.GuildList.Bumped();
			}
		}

		[Group(nameof(GetGuildListing)), ModuleInitialismAlias(typeof(GetGuildListing))]
		[LocalizedSummary(nameof(Summaries.GetGuildListing))]
		[Meta("5b004c37-2629-4f8a-a10a-8397413e275e")]
		public sealed class GetGuildListing : AdvobotModuleBase
		{
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
			public IInviteListService Invites { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.

			[Command]
			public Task<RuntimeResult> Command([Remainder] ListedInviteFilterer filterer)
			{
				var invites = (filterer.Keywords.Count > 0
					? Invites.GetAll(int.MaxValue, filterer.Keywords)
					: Invites.GetAll(int.MaxValue)).Where(x => !x.Expired);
				var matches = filterer.Filter(invites);
				if (matches.Count == 0)
				{
					return Responses.GuildList.NoInviteMatch();
				}
				if (matches.Count <= 50)
				{
					return Responses.GuildList.InviteMatches(matches);
				}
				return Responses.GuildList.TooManyMatches();
			}

			[NamedArgumentType]
			public sealed class ListedInviteFilterer : Filterer<IListedInvite>
			{
				public string? Code { get; set; }
				public string? Name { get; set; }
				public bool? HasGlobalEmotes { get; set; }
				public int? Users { get; set; }
				public CountTarget UsersMethod { get; set; }
				public IList<string> Keywords { get; set; } = new List<string>();

				public override IReadOnlyList<IListedInvite> Filter(
					IEnumerable<IListedInvite> source)
				{
					if (Code != null)
					{
						source = source.Where(x => x.Code == Code);
					}
					if (Name != null)
					{
						source = source.Where(x => x.GuildName.CaseInsEquals(Name));
					}
					if (HasGlobalEmotes != null)
					{
						source = source.Where(x => x.HasGlobalEmotes);
					}
					if (Users != null)
					{
						source = source.GetFromCount(UsersMethod, Users, x => x.GuildMemberCount);
					}
					return source?.ToArray() ?? Array.Empty<IListedInvite>();
				}
			}
		}
	}
}