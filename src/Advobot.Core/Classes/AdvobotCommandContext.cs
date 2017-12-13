using Advobot.Core.Interfaces;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// A <see cref="CommandContext"/> which contains <see cref="IBotSettings"/>, <see cref="IGuildSettings"/>, <see cref="ILogService"/>, and <see cref="ITimersService"/>.
	/// </summary>
	public class AdvobotCommandContext : CommandContext, IAdvobotCommandContext
	{
		public IBotSettings BotSettings { get; }
		public ILogService Logging { get; }
		public ITimersService Timers { get; }
		public IInviteListService InviteList { get; }
		public IGuildSettings GuildSettings { get; }

		public AdvobotCommandContext(IServiceProvider provider, IGuildSettings guildSettings, IDiscordClient client, IUserMessage msg) : base(client, msg)
		{
			BotSettings = provider.GetRequiredService<IBotSettings>();
			Logging = provider.GetRequiredService<ILogService>();
			Timers = provider.GetRequiredService<ITimersService>();
			InviteList = provider.GetRequiredService<IInviteListService>();
			GuildSettings = guildSettings;
		}

		public string GetPrefix() => GuildSettings.GetPrefix(BotSettings);
	}
}