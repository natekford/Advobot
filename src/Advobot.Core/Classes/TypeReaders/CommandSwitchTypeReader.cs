using Advobot.Core.Interfaces;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Advobot.Core.Classes.TypeReaders
{
	/// <summary>
	/// Attempts to find a <see cref="CommandSwitch"/> in the current guild settings.
	/// </summary>
	public sealed class CommandSwitchTypeReader : TypeReader
	{
		/// <summary>
		/// Finds a command with the supplied name.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="input"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
		{
			if (context is IAdvobotCommandContext advobotCommandContext)
			{
				var command = advobotCommandContext.GuildSettings.GetCommand(input);
				if (command != null)
				{
					return Task.FromResult(TypeReaderResult.FromSuccess(command));
				}
			}
			return Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound, "Unable to find a command matching the supplied input."));
		}
	}
}