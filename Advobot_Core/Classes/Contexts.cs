using Advobot.Attributes;
using Advobot.Interfaces;
using Discord;
using Discord.Commands;

namespace Advobot.Classes
{
	/// <summary>
	/// Same as MyModuleBase except saves guild settings afterwards.
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
	/// Shorter way to write ModuleBase<MyCommandContext> and also has every command go through the command requirements attribute first.
	/// </summary>
	[CommandRequirements]
	public class MyModuleBase : ModuleBase<MyCommandContext> { }

	public class MyCommandContext : CommandContext, IMyCommandContext
	{
		public IBotSettings BotSettings { get; }
		public IGuildSettings GuildSettings { get; }
		public ILogModule Logging { get; }
		public ITimersModule Timers { get; }

		public MyCommandContext(IBotSettings botSettings, IGuildSettings guildSettings, ILogModule logging, ITimersModule timers, IDiscordClient client, IUserMessage msg) : base(client, msg)
		{
			BotSettings = botSettings;
			GuildSettings = guildSettings;
			Logging = logging;
			Timers = timers;
		}
	}
}
