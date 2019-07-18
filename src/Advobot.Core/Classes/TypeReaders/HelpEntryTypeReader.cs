using System;
using System.Threading.Tasks;
using Advobot.Classes.Attributes;
using Advobot.Services.HelpEntries;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Classes.TypeReaders
{
	/// <summary>
	/// Attempts to find a help entry with the supplied name.
	/// </summary>
	[TypeReaderTargetType(typeof(IHelpEntry))]
	public sealed class HelpEntryTypeReader : TypeReader
	{
		/// <summary>
		/// Attempts to find a help entry with the supplied input as a name.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="input"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
		{
			return services.GetRequiredService<IHelpEntryService>().TryGetValue(input, out var entry)
				? Task.FromResult(TypeReaderResult.FromSuccess(entry))
				: Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound, $"Unable to find a command matching `{input}`."));
		}
	}
}