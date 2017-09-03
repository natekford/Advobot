using Advobot.Actions;
using Advobot.Classes;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Advobot.TypeReaders
{
	public class BypassUserLimitTypeReader : TypeReader
	{
		public override Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
		{
			return Task.FromResult(TypeReaderResult.FromSuccess(Constants.BYPASS_STRING.CaseInsEquals(input)));
		}
	}

	public class CommandSwitchTypeReader : TypeReader
	{
		public override Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
		{
			if (context is MyCommandContext)
			{
				var cont = context as MyCommandContext;
				var command = GetActions.GetCommand(cont.GuildSettings, input);
				if (command != null)
				{
					return Task.FromResult(TypeReaderResult.FromSuccess(command));
				}
			}
			return Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound, "Unable to find a command matching the supplied input."));
		}
	}
}
