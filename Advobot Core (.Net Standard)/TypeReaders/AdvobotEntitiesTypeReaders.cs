using Advobot.Actions;
using Advobot.Classes;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Advobot.TypeReaders
{
	/// <summary>
	/// Attempts to see if the input matches <see cref="Constants.BYPASS_STRING"/>.
	/// </summary>
	internal class BypassUserLimitTypeReader : TypeReader
	{
		/// <summary>
		/// Returns true if the input is equal to <see cref="Constants.BYPASS_STRING"/>.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="input"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
		{
			return Task.FromResult(TypeReaderResult.FromSuccess(Constants.BYPASS_STRING.CaseInsEquals(input)));
		}
	}

	/// <summary>
	/// Attempts to find a <see cref="CommandSwitch"/> in the current guild settings.
	/// </summary>
	internal class CommandSwitchTypeReader : TypeReader
	{
		public override Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
		{
			if (context is MyCommandContext)
			{
				var command = GetActions.GetCommand((context as MyCommandContext).GuildSettings, input);
				if (command != null)
				{
					return Task.FromResult(TypeReaderResult.FromSuccess(command));
				}
			}
			return Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound, "Unable to find a command matching the supplied input."));
		}
	}
}
