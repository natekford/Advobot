using Advobot.Attributes;
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