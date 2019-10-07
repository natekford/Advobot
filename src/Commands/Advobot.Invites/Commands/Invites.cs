using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Advobot.Attributes;
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Invites;
using Advobot.Attributes.Preconditions.Permissions;
using Advobot.Classes;
using Advobot.Invites.Localization;
using Advobot.Invites.Preconditions;
using Advobot.Invites.ReadOnlyModels;
using Advobot.Invites.Resources;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

namespace Advobot.Invites.Commands
{
	[Category(nameof(Invites))]
	[LocalizedGroup(nameof(Groups.Invites))]
	[LocalizedAlias(nameof(Aliases.Invites))]
	public sealed class Invites : ModuleBase
	{
		[LocalizedGroup(nameof(Groups.Bump))]
		[LocalizedAlias(nameof(Aliases.Bump))]
		[LocalizedSummary(nameof(Summaries.Bump))]
		[Meta("7522e03e-a53a-4ac6-b522-54db5b7b0d05")]
		[RequireGenericGuildPermissions]
		[RequireNotRecentlyBumped]
		public sealed class InvitesBump : InviteModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command()
			{
				await Invites.BumpAsync(Context.Guild).CAF();
				return Responses.ListedInvites.Bumped();
			}
		}

		[LocalizedGroup(nameof(Groups.Get))]
		[LocalizedAlias(nameof(Aliases.Get))]
		[LocalizedSummary(nameof(Summaries.Get))]
		[Meta("5b004c37-2629-4f8a-a10a-8397413e275e")]
		public sealed class InvitesGet : InviteModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command([Remainder] ListedInviteFilterer filterer)
			{
				var invites = await (filterer.Keywords.Count > 0
					? Invites.GetAllAsync(filterer.Keywords)
					: Invites.GetAllAsync()).CAF();
				var matches = filterer.Filter(invites);
				if (matches.Count == 0)
				{
					return Responses.ListedInvites.NoInviteMatch();
				}
				else if (matches.Count <= 50)
				{
					return Responses.ListedInvites.InviteMatches(matches);
				}
				return Responses.ListedInvites.TooManyMatches();
			}

			[NamedArgumentType]
			public sealed class ListedInviteFilterer : Filterer<IReadOnlyListedInvite>
			{
				public string? Code { get; set; }
				public bool? HasGlobalEmotes { get; set; }
				public IList<string> Keywords { get; set; } = new List<string>();
				public string? Name { get; set; }
				public int? Users { get; set; }
				public CountTarget UsersMethod { get; set; }

				public override IReadOnlyList<IReadOnlyListedInvite> Filter(
					IEnumerable<IReadOnlyListedInvite> source)
				{
					if (Code != null)
					{
						source = source.Where(x => x.Code == Code);
					}
					if (Name != null)
					{
						source = source.Where(x => x.Name.CaseInsEquals(Name));
					}
					if (HasGlobalEmotes != null)
					{
						source = source.Where(x => x.HasGlobalEmotes);
					}
					if (Users != null)
					{
						source = source.GetFromCount(UsersMethod, Users, x => x.MemberCount);
					}
					return source?.ToArray() ?? Array.Empty<IReadOnlyListedInvite>();
				}
			}
		}

		[LocalizedGroup(nameof(Groups.Modify))]
		[LocalizedAlias(nameof(Aliases.Modify))]
		[LocalizedSummary(nameof(Summaries.Modify))]
		[Meta("ad81af3b-c2d7-4e49-9cef-2be7f0c6cf9e")]
		[RequireGuildPermissions]
		public sealed class InvitesModify : InviteModuleBase
		{
			[LocalizedCommand(nameof(Groups.Add))]
			[LocalizedAlias(nameof(Aliases.Add))]
			public async Task<RuntimeResult> Add(
				[NeverExpires, FromThisGuild] IInviteMetadata invite)
			{
				await Invites.AddInviteAsync(invite).CAF();
				return Responses.ListedInvites.CreatedListing(invite);
			}

			[LocalizedCommand(nameof(Groups.Remove))]
			[LocalizedAlias(nameof(Aliases.Remove))]
			public async Task<RuntimeResult> Remove()
			{
				await Invites.RemoveInviteAsync(Context.Guild.Id).CAF();
				return Responses.ListedInvites.DeletedListing();
			}
		}
	}
}