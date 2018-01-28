using System;
using System.Diagnostics;
using Advobot.Core.Interfaces;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// A <see cref="CommandContext"/> which contains <see cref="IBotSettings"/>, <see cref="IGuildSettings"/>, <see cref="ILogService"/>, and <see cref="ITimersService"/>.
	/// </summary>
	public sealed class AdvobotCommandContext : CommandContext, IAdvobotCommandContext
	{
		public IBotSettings BotSettings { get; }
		public ILogService Logging { get; }
		public ITimersService Timers { get; }
		public IInviteListService InviteList { get; }
		public IGuildSettings GuildSettings { get; }

		private Stopwatch _Stopwatch = new Stopwatch();
		public long ElapsedMilliseconds => _Stopwatch.ElapsedMilliseconds;

		public AdvobotCommandContext(IServiceProvider provider, IGuildSettings guildSettings, IDiscordClient client, IUserMessage msg) : base(client, msg)
		{
			_Stopwatch.Start();
			BotSettings = provider.GetRequiredService<IBotSettings>();
			Logging = provider.GetRequiredService<ILogService>();
			Timers = provider.GetRequiredService<ITimersService>();
			InviteList = provider.GetRequiredService<IInviteListService>();
			GuildSettings = guildSettings;
		}

		public string GetPrefix()
		{
			return String.IsNullOrWhiteSpace(GuildSettings.Prefix) ? BotSettings.Prefix : GuildSettings.Prefix;
		}
	}
}