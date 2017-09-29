using Advobot.Classes.Attributes;
using Advobot.Interfaces;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Advobot.Classes
{
	/// <summary>
	/// Same as <see cref="MyModuleBase"/> except saves guild settings afterwards.
	/// </summary>
	public class MySavingModuleBase : MyModuleBase
	{
		protected override void AfterExecute(CommandInfo command)
		{
			Context.GuildSettings.SaveSettings();
			base.AfterExecute(command);
		}
	}

	/// <summary>
	/// Shorter way to write ModuleBase<MyCommandContext> and also has every command go through the <see cref="CommandRequirementsAttribute"/> first.
	/// </summary>
	[CommandRequirements]
	public class MyModuleBase : ModuleBase<MyCommandContext> { }

	/// <summary>
	/// A <see cref="CommandContext"/> which contains <see cref="IBotSettings"/>, <see cref="IGuildSettings"/>, <see cref="ILogModule"/>, and <see cref="ITimersModule"/>.
	/// </summary>
	public class MyCommandContext : CommandContext, IMyCommandContext
	{
		private IServiceProvider _Provider;

		public IBotSettings BotSettings	=> _Provider.GetService<IBotSettings>();
		public ILogModule Logging		=> _Provider.GetService<ILogModule>();
		public ITimersModule Timers		=> _Provider.GetService<ITimersModule>();
		public IGuildSettings GuildSettings { get; }

		public MyCommandContext(IServiceProvider provider, IGuildSettings guildSettings, IDiscordClient client, IUserMessage msg) : base(client, msg)
		{
			_Provider = provider;
			GuildSettings = guildSettings;
		}
	}
}
