using Advobot.Core.Actions;
using Advobot.Core.Interfaces;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Core.Classes.TypeReaders
{
	/// <summary>
	/// Attempts to find a rule category in the guild settings.
	/// </summary>
	public sealed class RuleCategoryTypeReader : TypeReader
	{
		/// <summary>
		/// Attempts to find a category with the supplied input as a name.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="input"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
		{
			if (context is IAdvobotCommandContext advobotCommandContext)
			{
				var command = advobotCommandContext.GuildSettings.Rules.Categories.SingleOrDefault(x => x.Name.CaseInsEquals(input));
				if (command != null)
				{
					return Task.FromResult(TypeReaderResult.FromSuccess(command));
				}
			}
			return Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound, "Unable to find a rule category matching the supplied input."));
		}
	}
}
