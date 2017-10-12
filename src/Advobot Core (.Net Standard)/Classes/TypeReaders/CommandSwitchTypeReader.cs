using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Advobot.Classes.TypeReaders
{
	/// <summary>
	/// Attempts to find a <see cref="CommandSwitch"/> in the current guild settings.
	/// </summary>
	public sealed class CommandSwitchTypeReader : TypeReader
	{
		public override Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
		{
			if (context is AdvobotCommandContext myContext)
			{
				var command = myContext.GuildSettings.GetCommand(input);
				if (command != null)
				{
					return Task.FromResult(TypeReaderResult.FromSuccess(command));
				}
			}
			return Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound, "Unable to find a command matching the supplied input."));
		}
	}
}