using Advobot.Core;
using Advobot.Core.Utilities;
using Advobot.Core.Utilities.Formatting;
using Advobot.Core.Classes;
using Advobot.Core.Classes.Attributes;
using Advobot.Core.Classes.Permissions;
using Advobot.Core.Classes.TypeReaders;
using Advobot.Core.Enums;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Core.Interfaces;
using System.Collections.Generic;

namespace Advobot.Commands.CategoryModeration
{
	/* TODO: uncomment when channel categories are more supported in the discord library. Mainly in ChannelPermissions.All so that doesn't throw an exception
	[Group(nameof(CreateCategory)), TopLevelShortAlias(typeof(CreateCategory))]
	[Summary("Creates an empty category and puts it at the bottom of the channel list.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class CreateCategory : AdvobotModuleBase
	{
		[Command]
		public async Task Command([Remainder, VerifyStringLength(Target.Channel)] string name)
		{
			var channel = await ChannelUtils.CreateCategoryAsync(Context.Guild, name, new ModerationReason(Context.User, null)).CAF();
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully created `{channel.FormatChannel()}`.").CAF();
		}
	}

	[Group(nameof(SoftDeleteCategory)), TopLevelShortAlias(typeof(SoftDeleteCategory))]
	[Summary("Makes everyone unable to see the category and moves it to the bottom of the channel list.")]
	[PermissionRequirement(null, new[] { GuildPermission.ManageChannels, GuildPermission.ManageRoles })]
	[DefaultEnabled(true)]
	public sealed class SoftDeleteCategory : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeManaged)] ICategoryChannel channel)
		{
			await ChannelUtils.SoftDeleteChannelAsync(channel, new ModerationReason(Context.User, null)).CAF();
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully softdeleted `{channel.FormatChannel()}`.").CAF();
		}
	}*/
}
