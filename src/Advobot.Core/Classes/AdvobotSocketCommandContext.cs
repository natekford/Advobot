using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// A <see cref="SocketCommandContext"/> which contains <see cref="IBotSettings"/>, <see cref="IGuildSettings"/>, <see cref="ILogService"/>, and <see cref="ITimersService"/>.
	/// </summary>
	public class AdvobotSocketCommandContext : SocketCommandContext
	{
		public IBotSettings BotSettings { get; }
		public ILogService Logging { get; }
		public ITimersService Timers { get; }
		public IInviteListService InviteList { get; }
		public IGuildSettings GuildSettings { get; }
		public HelpEntryHolder HelpEntries { get; }

		private Stopwatch _Stopwatch = new Stopwatch();
		public long ElapsedMilliseconds => _Stopwatch.ElapsedMilliseconds;

		public AdvobotSocketCommandContext(IServiceProvider provider, IGuildSettings settings, DiscordSocketClient client, SocketUserMessage msg) : base(client, msg)
		{
			_Stopwatch.Start();
			BotSettings = provider.GetRequiredService<IBotSettings>();
			Logging = provider.GetRequiredService<ILogService>();
			Timers = provider.GetRequiredService<ITimersService>();
			InviteList = provider.GetRequiredService<IInviteListService>();
			HelpEntries = provider.GetRequiredService<HelpEntryHolder>();
			GuildSettings = settings;
		}

		public string GetPrefix()
		{
			return String.IsNullOrWhiteSpace(GuildSettings.Prefix) ? BotSettings.Prefix : GuildSettings.Prefix;
		}
	}
}