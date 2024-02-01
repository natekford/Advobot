using Advobot.Attributes;
using Advobot.Classes;
using Advobot.Localization;
using Advobot.Modules;
using Advobot.ParameterPreconditions.DiscordObjectValidation.Channels;
using Advobot.ParameterPreconditions.DiscordObjectValidation.Invites;
using Advobot.Preconditions.Permissions;
using Advobot.Resources;
using Advobot.TypeReaders;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

using static Discord.ChannelPermission;

namespace Advobot.Standard.Commands;

[Category(nameof(Invites))]
public sealed class Invites : ModuleBase
{
	[LocalizedGroup(nameof(Groups.CreateInvite))]
	[LocalizedAlias(nameof(Aliases.CreateInvite))]
	[LocalizedSummary(nameof(Summaries.CreateInvite))]
	[Meta("6e8233c0-c8f4-456a-85e4-6f5203add299", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.CreateInstantInvite)]
	public sealed class CreateInvite : AdvobotModuleBase
	{
		[Command]
		public Task<RuntimeResult> Command(
			[CanModifyChannel(CreateInstantInvite)] ITextChannel channel,
			CreateInviteArguments? arguments = null)
			=> CommandRunner(channel, arguments);

		[Command]
		public Task<RuntimeResult> Command(
			[CanModifyChannel(CreateInstantInvite)] IVoiceChannel channel,
			CreateInviteArguments? arguments = null)
			=> CommandRunner(channel, arguments);

		private async Task<RuntimeResult> CommandRunner(
			INestedChannel channel,
			CreateInviteArguments? args)
		{
			args ??= new();
			var options = GetOptions();
			var invite = await channel.CreateInviteAsync(args.Time, args.Uses, args.IsTemporary, args.IsUnique, options).CAF();
			return Responses.Snowflakes.Created(invite);
		}

		[NamedArgumentType]
		public sealed class CreateInviteArguments
		{
			/// <summary>
			/// Whether the user only receives temporary membership from the invite.
			/// </summary>
			public bool IsTemporary { get; set; }
			/// <summary>
			/// Whether the invite should be unique.
			/// </summary>
			public bool IsUnique { get; set; }
			/// <summary>
			/// How long to make the invite last for.
			/// </summary>
			[OverrideTypeReader(typeof(PositiveNullableIntTypeReader))]
			public int? Time { get; set; } = 86400;
			/// <summary>
			/// How many uses to let the invite last for.
			/// </summary>
			[OverrideTypeReader(typeof(PositiveNullableIntTypeReader))]
			public int? Uses { get; set; }
		}
	}

	[LocalizedGroup(nameof(Groups.DeleteInvite))]
	[LocalizedAlias(nameof(Aliases.DeleteInvite))]
	[LocalizedSummary(nameof(Summaries.DeleteInvite))]
	[Meta("993e5613-6cdb-4ff3-925d-98e3a534ddc8", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.ManageChannels)]
	public sealed class DeleteInvite : AdvobotModuleBase
	{
		[Command]
		public async Task<RuntimeResult> Command([FromThisGuild] IInviteMetadata invite)
		{
			await invite.DeleteAsync(GetOptions()).CAF();
			return Responses.Snowflakes.Deleted(invite);
		}
	}

	[LocalizedGroup(nameof(Groups.DeleteMultipleInvites))]
	[LocalizedAlias(nameof(Aliases.DeleteMultipleInvites))]
	[LocalizedSummary(nameof(Summaries.DeleteMultipleInvites))]
	[Meta("a53c0e51-d580-436e-869c-e566ff268c3e", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.ManageChannels)]
	public sealed class DeleteMultipleInvites : AdvobotModuleBase
	{
		[Command(RunMode = RunMode.Async)]
		public async Task<RuntimeResult> Command([Remainder] InviteFilterer filterer)
		{
			var invites = await Context.Guild.GetInvitesAsync().CAF();
			var filtered = filterer.Filter(invites);
			if (filtered.Count == 0)
			{
				return Responses.Invites.NoInviteMatches();
			}

			foreach (var invite in filtered)
			{
				await invite.DeleteAsync(GetOptions()).CAF();
			}
			return Responses.Invites.DeletedMultipleInvites(filtered);
		}

		[NamedArgumentType]
		public sealed class InviteFilterer : Filterer<IInviteMetadata>
		{
			public int? Age { get; set; }
			public CountTarget AgeMethod { get; set; }
			public ulong? ChannelId { get; set; }
			public bool? IsTemporary { get; set; }
			public bool? NeverExpires { get; set; }
			public bool? NoMaxUses { get; set; }
			public ulong? UserId { get; set; }
			public int? Uses { get; set; }
			public CountTarget UsesMethod { get; set; }

			public override IReadOnlyList<IInviteMetadata> Filter(
				IEnumerable<IInviteMetadata> source)
			{
				if (UserId != null)
				{
					source = source.Where(x => x.Inviter.Id == UserId);
				}
				if (ChannelId != null)
				{
					source = source.Where(x => x.ChannelId == ChannelId);
				}
				if (Uses != null)
				{
					source = source.GetFromCount(UsesMethod, Uses, x => x.Uses);
				}
				if (Age != null)
				{
					source = source.GetFromCount(AgeMethod, Age, x => x.MaxAge);
				}
				if (IsTemporary != null)
				{
					source = source.Where(x => x.IsTemporary == IsTemporary);
				}
				if (NeverExpires != null)
				{
					source = source.Where(x => x.MaxAge == null == NeverExpires);
				}
				if (NoMaxUses != null)
				{
					source = source.Where(x => x.MaxUses == null == NoMaxUses);
				}
				return source?.ToArray() ?? [];
			}
		}
	}

	[LocalizedGroup(nameof(Groups.DisplayInvites))]
	[LocalizedAlias(nameof(Aliases.DisplayInvites))]
	[LocalizedSummary(nameof(Summaries.DisplayInvites))]
	[Meta("958c8da4-352e-468e-8279-0fd80276cd24", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.ManageGuild)]
	public sealed class DisplayInvites : AdvobotModuleBase
	{
		[Command]
		public async Task<RuntimeResult> Command()
		{
			var invites = await Context.Guild.GetInvitesAsync().CAF();
			var ordered = invites.OrderByDescending(x => x.Uses).ToArray();
			return Responses.Invites.DisplayInvites(ordered);
		}
	}
}