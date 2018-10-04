﻿using System;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.Attributes.ParameterPreconditions.StringValidation;
using Advobot.Classes.Attributes.Preconditions;
using Advobot.Classes.ImageResizing;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;

namespace Advobot.Commands.Client
{
	[Category(typeof(ModifyBotName)), Group(nameof(ModifyBotName)), TopLevelShortAlias(typeof(ModifyBotName))]
	[Summary("Changes the bot's name to the given name.")]
	[RequireBotOwner]
	[DefaultEnabled(true)]
	public sealed class ModifyBotName : AdvobotModuleBase
	{
		[Command]
		public async Task Command([Remainder, ValidateUsername] string name)
		{
			await Context.Client.CurrentUser.ModifyAsync(x => x.Username = name).CAF();
			await ReplyTimedAsync($"Successfully changed my username to `{name}`.").CAF();
		}
	}

	[Category(typeof(ModifyBotIcon)), Group(nameof(ModifyBotIcon)), TopLevelShortAlias(typeof(ModifyBotIcon))]
	[Summary("Changes the bot's icon to the given image. " +
		"The image must be smaller than 2.5MB.")]
	[RequireBotOwner]
	[DefaultEnabled(true)]
	public sealed class ModifyBotIcon : ImageResizerModule
	{
		[Command]
		public async Task Command(Uri url)
		{
			await ProcessAsync(new IconCreationArgs("Bot Icon", Context, url, default, async (ctx, ms) =>
			{
				await ctx.Client.CurrentUser.ModifyAsync(x => x.Avatar = new Image(ms), ctx.GenerateRequestOptions()).CAF();
			})).CAF();
		}
		[Command(nameof(Remove)), ShortAlias(nameof(Remove))]
		public async Task Remove()
		{
			await Context.Client.CurrentUser.ModifyAsync(x => x.Avatar = new Image()).CAF();
			await ReplyTimedAsync("Successfully removed the bot icon.").CAF();
		}
	}

	[Category(typeof(DisconnectBot)), Group(nameof(DisconnectBot)), TopLevelShortAlias(typeof(DisconnectBot), "runescapeservers")]
	[Summary("Turns the bot off.")]
	[RequireBotOwner]
	[DefaultEnabled(true)]
	public sealed class DisconnectBot : AdvobotModuleBase
	{
		[Command(RunMode = RunMode.Async)]
		public async Task Command()
			=> await ClientUtils.DisconnectBotAsync(Context.Client).CAF();
	}

	[Category(typeof(RestartBot)), Group(nameof(RestartBot)), TopLevelShortAlias(typeof(RestartBot))]
	[Summary("Restarts the bot.")]
	[RequireBotOwner]
	[DefaultEnabled(true)]
	public sealed class RestartBot : AdvobotModuleBase
	{
		[Command(RunMode = RunMode.Async)]
		public async Task Command()
			=> await ClientUtils.RestartBotAsync(Context.Client, BotSettings).CAF();
	}
}