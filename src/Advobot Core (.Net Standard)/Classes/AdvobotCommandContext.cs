using Advobot.Interfaces;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Advobot.Classes
{
	/// <summary>
	/// A <see cref="CommandContext"/> which contains <see cref="IBotSettings"/>, <see cref="IGuildSettings"/>, <see cref="ILogService"/>, and <see cref="ITimersService"/>.
	/// </summary>
	public class AdvobotCommandContext : CommandContext, IAdvobotCommandContext
	{
		private IServiceProvider _Provider;

		public IBotSettings BotSettings => _Provider.GetService<IBotSettings>();
		public ILogService Logging => _Provider.GetService<ILogService>();
		public ITimersService Timers => _Provider.GetService<ITimersService>();
		public IInviteListService InviteList => _Provider.GetService<IInviteListService>();
		public IGuildSettings GuildSettings { get; }

		public AdvobotCommandContext(IServiceProvider provider, IGuildSettings guildSettings, IDiscordClient client, IUserMessage msg) : base(client, msg)
		{
			_Provider = provider;
			GuildSettings = guildSettings;
		}
	}
}