using Advobot.Core.Utilities;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Core.Classes.TypeReaders
{
	public sealed class CommandCategoryTypeReader : TypeReader
	{
		/// <summary>
		/// Returns the category with the supplied name.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="input"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
		{
			var category = Constants.HELP_ENTRIES.GetCategories().FirstOrDefault(x => x.Name.CaseInsEquals(input));
			return category.Name != null
				? Task.FromResult(TypeReaderResult.FromSuccess(category))
				: Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound, $"{input} is not a valid command category."));
		}
	}
}
