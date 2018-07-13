using AdvorangesUtils;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Classes.TypeReaders
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
			if (context is AdvobotSocketCommandContext advobotCommandContext
				&& advobotCommandContext.GuildSettings.Rules?.Categories?.FirstOrDefault(x => x.Key.Name.CaseInsEquals(input)) != null)
			{
				return Task.FromResult(TypeReaderResult.FromSuccess(input));
			}
			return Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound, "Unable to find a rule category matching the supplied input."));
		}
	}
}
