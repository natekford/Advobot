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
			this.BotSettings = provider.GetRequiredService<IBotSettings>();
			this.Logging = provider.GetRequiredService<ILogService>();
			this.Timers = provider.GetRequiredService<ITimersService>();
			this.InviteList = provider.GetRequiredService<IInviteListService>();
			this.GuildSettings = guildSettings;
		}
	}
}